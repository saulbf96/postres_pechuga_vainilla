using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Services;

public class PedidoService : IPedidoService
{
    private readonly PechugaVainillaDbContext _context;

    public PedidoService(PechugaVainillaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pedido>> CrearPedidoAsync(SolicitudCheckout solicitud, string usuarioId, CancellationToken cancellationToken)
    {
        if (solicitud.Grupos.Count == 0)
        {
            throw new ReglaDeNegocioException("El carrito esta vacio.");
        }

        // Mismo CheckoutId para todos los pedidos que salen de esta compra (uno por vendedor).
        var checkoutId = Guid.NewGuid().ToString("N");
        var pedidosCreados = new List<Pedido>();

        foreach (var grupo in solicitud.Grupos)
        {
            if (grupo.Items.Count == 0)
            {
                throw new ReglaDeNegocioException("Cada grupo del carrito debe tener al menos un producto.");
            }

            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == grupo.VendedorId && v.Activo, cancellationToken)
                ?? throw new ReglaDeNegocioException("El vendedor ya no esta disponible.");

            var puntoEntrega = await _context.PuntosEntrega
                .FirstOrDefaultAsync(pe => pe.Id == grupo.PuntoEntregaId && pe.VendedorId == grupo.VendedorId && pe.Activo, cancellationToken)
                ?? throw new ReglaDeNegocioException("El punto de entrega elegido no es valido para ese vendedor.");

            ValidarDiaYHora(puntoEntrega, grupo.FechaEntrega, grupo.HoraEntrega);

            var pedido = new Pedido
            {
                CheckoutId = checkoutId,
                UsuarioId = usuarioId,
                NombreCliente = solicitud.NombreCliente,
                WhatsApp = solicitud.WhatsApp,
                VendedorId = vendedor.Id,
                Vendedor = vendedor,
                PuntoEntregaId = puntoEntrega.Id,
                PuntoEntrega = puntoEntrega,
                FechaEntrega = grupo.FechaEntrega,
                HoraEntrega = grupo.HoraEntrega,
                DetalleEntrega = solicitud.DetalleEntrega,
            };

            var totalGrupo = 0m;

            foreach (var item in grupo.Items)
            {
                if (item.Cantidad <= 0)
                {
                    throw new ReglaDeNegocioException("La cantidad debe ser mayor a cero.");
                }

                var presentacion = await _context.Presentaciones
                    .Include(pr => pr.Producto)
                    .FirstOrDefaultAsync(
                        pr => pr.Id == item.PresentacionId && pr.ProductoId == item.ProductoId && pr.Activo,
                        cancellationToken)
                    ?? throw new ReglaDeNegocioException("Un producto del carrito ya no esta disponible.");

                if (!presentacion.Producto.Activo || presentacion.Producto.VendedorId != grupo.VendedorId)
                {
                    throw new ReglaDeNegocioException("Un producto del carrito ya no esta disponible.");
                }

                if (presentacion.Producto.MaxPorDia.HasValue)
                {
                    var yaVendido = await _context.PedidoDetalles
                        .Where(pd => pd.ProductoId == item.ProductoId
                            && pd.Pedido.FechaEntrega == grupo.FechaEntrega
                            && pd.Pedido.Estado != EstadoPedido.Cancelado)
                        .SumAsync(pd => (int?)pd.Cantidad, cancellationToken) ?? 0;

                    if (yaVendido + item.Cantidad > presentacion.Producto.MaxPorDia.Value)
                    {
                        throw new ReglaDeNegocioException(
                            $"Ya no hay suficiente '{presentacion.Producto.Nombre}' disponible para ese dia.");
                    }
                }

                pedido.Detalles.Add(new PedidoDetalle
                {
                    Pedido = pedido,
                    ProductoId = presentacion.ProductoId,
                    PresentacionId = presentacion.Id,
                    NombreProducto = $"{presentacion.Producto.Nombre} ({presentacion.Nombre})",
                    PrecioUnitario = presentacion.Precio, // precio del servidor, nunca el que mande el navegador
                    Cantidad = item.Cantidad,
                    Notas = item.Notas,
                });

                totalGrupo += presentacion.Precio * item.Cantidad;
            }

            totalGrupo += puntoEntrega.CostoEnvio;
            pedido.Total = totalGrupo;

            pedido.Pago = new Pago
            {
                Pedido = pedido,
                Metodo = solicitud.MetodoPago,
                // Sin pasarela todavia (Etapa 4): efectivo se cobra al entregar, transferencia se confirma a mano.
                Estado = solicitud.MetodoPago == MetodoPago.Efectivo ? EstadoPago.PorCobrar : EstadoPago.PorConfirmar,
                Monto = totalGrupo,
            };

            _context.Pedidos.Add(pedido);
            pedidosCreados.Add(pedido);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return pedidosCreados;
    }

    public async Task<IReadOnlyList<Pedido>> ObtenerMisPedidosAsync(string usuarioId, CancellationToken cancellationToken)
    {
        return await ConsultaConDetalles()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pedido?> ObtenerPedidoAsync(int id, string usuarioId, CancellationToken cancellationToken)
    {
        // El filtro por UsuarioId aqui mismo es la autorizacion por dueño: un cliente jamas
        // puede leer un pedido de otro usuario, ni cambiando el id en la URL.
        return await ConsultaConDetalles()
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<IReadOnlyList<Pedido>> ObtenerPorVendedorAsync(int? vendedorId, CancellationToken cancellationToken)
    {
        var query = ConsultaConDetalles();

        if (vendedorId.HasValue)
        {
            query = query.Where(p => p.VendedorId == vendedorId.Value);
        }

        return await query.OrderByDescending(p => p.CreadoEn).ToListAsync(cancellationToken);
    }

    public async Task CambiarEstadoAsync(int id, int? vendedorIdPermitido, EstadoPedido nuevoEstado, CancellationToken cancellationToken)
    {
        var pedido = await ObtenerConPermisoAsync(id, vendedorIdPermitido, cancellationToken);
        pedido.Estado = nuevoEstado;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarEstadoPagoAsync(int id, int? vendedorIdPermitido, EstadoPago nuevoEstado, CancellationToken cancellationToken)
    {
        var pedido = await ObtenerConPermisoAsync(id, vendedorIdPermitido, cancellationToken);

        if (pedido.Pago is null)
        {
            throw new ReglaDeNegocioException("Este pedido no tiene un pago asociado.");
        }

        pedido.Pago.Estado = nuevoEstado;
        if (nuevoEstado is EstadoPago.Pagado or EstadoPago.Cobrado)
        {
            pedido.Pago.PagadoEn = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Pedido> ObtenerConPermisoAsync(int id, int? vendedorIdPermitido, CancellationToken cancellationToken)
    {
        var pedido = await ConsultaConDetalles().FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new ReglaDeNegocioException("El pedido no existe.");

        if (vendedorIdPermitido.HasValue && pedido.VendedorId != vendedorIdPermitido.Value)
        {
            throw new ReglaDeNegocioException("No tienes permiso sobre este pedido.");
        }

        return pedido;
    }

    private IQueryable<Pedido> ConsultaConDetalles() =>
        _context.Pedidos
            .Include(p => p.Vendedor)
            .Include(p => p.PuntoEntrega)
            .Include(p => p.Detalles)
            .Include(p => p.Pago);

    private static void ValidarDiaYHora(PuntoEntrega puntoEntrega, DateOnly fecha, TimeOnly hora)
    {
        var diaFlag = DiaSemanaAFlag(fecha.DayOfWeek);
        if (!puntoEntrega.DiasSemana.HasFlag(diaFlag))
        {
            throw new ReglaDeNegocioException("Ese punto de entrega no atiende ese dia de la semana.");
        }

        if (hora < puntoEntrega.HoraInicio || hora > puntoEntrega.HoraFin)
        {
            throw new ReglaDeNegocioException("Esa hora esta fuera del horario de este punto de entrega.");
        }

        var zonaMexico = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
        var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaMexico);

        var fechaLimite = fecha.AddDays(-puntoEntrega.DiasAnticipacion);
        var limiteLocal = fechaLimite.ToDateTime(puntoEntrega.HoraLimitePedido);

        if (ahoraLocal > limiteLocal)
        {
            throw new ReglaDeNegocioException(
                $"Ya paso la hora limite para pedir para el {fecha:dd/MM/yyyy} en este punto de entrega.");
        }
    }

    private static DiasSemana DiaSemanaAFlag(DayOfWeek dia) => dia switch
    {
        DayOfWeek.Monday => DiasSemana.Lunes,
        DayOfWeek.Tuesday => DiasSemana.Martes,
        DayOfWeek.Wednesday => DiasSemana.Miercoles,
        DayOfWeek.Thursday => DiasSemana.Jueves,
        DayOfWeek.Friday => DiasSemana.Viernes,
        DayOfWeek.Saturday => DiasSemana.Sabado,
        DayOfWeek.Sunday => DiasSemana.Domingo,
        _ => DiasSemana.Ninguno,
    };
}
