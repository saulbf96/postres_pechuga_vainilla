using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Core.Interfaces;

public record ItemCarrito(int ProductoId, int PresentacionId, int Cantidad, string? Notas);

// Un checkout se divide en un GrupoCheckout por vendedor (regla de negocio: un pedido por vendedor).
public record GrupoCheckout(int VendedorId, int PuntoEntregaId, DateOnly FechaEntrega, TimeOnly HoraEntrega, IReadOnlyList<ItemCarrito> Items);

public record SolicitudCheckout(string NombreCliente, string WhatsApp, string? DetalleEntrega, MetodoPago MetodoPago, IReadOnlyList<GrupoCheckout> Grupos);

// Excepcion para reglas de negocio violadas (carrito vacio, fuera de horario, etc.) - el
// controller la atrapa y regresa 400 con el mensaje, nunca un 500 con detalles internos.
public class ReglaDeNegocioException(string mensaje) : Exception(mensaje);

public interface IPedidoService
{
    Task<IReadOnlyList<Pedido>> CrearPedidoAsync(SolicitudCheckout solicitud, string usuarioId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Pedido>> ObtenerMisPedidosAsync(string usuarioId, CancellationToken cancellationToken);

    Task<Pedido?> ObtenerPedidoAsync(int id, string usuarioId, CancellationToken cancellationToken);

    // Panel: vendedorId null = Administrador (ve todos); con valor = Vendedor (solo los suyos).
    Task<IReadOnlyList<Pedido>> ObtenerPorVendedorAsync(int? vendedorId, CancellationToken cancellationToken);

    // vendedorIdPermitido null = Administrador (puede cambiar cualquiera).
    Task CambiarEstadoAsync(int id, int? vendedorIdPermitido, EstadoPedido nuevoEstado, CancellationToken cancellationToken);

    Task CambiarEstadoPagoAsync(int id, int? vendedorIdPermitido, EstadoPago nuevoEstado, CancellationToken cancellationToken);
}
