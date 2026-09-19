using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Api.Controllers;

// [Authorize] a nivel de controller: TODO endpoint aqui requiere sesion iniciada.
// El carrito/checkout/seguimiento son solo para usuarios con cuenta (regla de negocio);
// los invitados siguen usando el boton de WhatsApp del catalogo.
[ApiController]
[Route("api/v1/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;
    private readonly UserManager<Usuario> _userManager;

    public PedidosController(IPedidoService pedidoService, UserManager<Usuario> userManager)
    {
        _pedidoService = pedidoService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<ActionResult<IReadOnlyList<PedidoDto>>> Crear(CheckoutRequest request, CancellationToken cancellationToken)
    {
        var usuarioId = _userManager.GetUserId(User)!;

        var solicitud = new SolicitudCheckout(
            request.NombreCliente,
            request.WhatsApp,
            request.DetalleEntrega,
            request.MetodoPago,
            request.Grupos.Select(g => new GrupoCheckout(
                g.VendedorId,
                g.PuntoEntregaId,
                g.FechaEntrega,
                g.HoraEntrega,
                g.Items.Select(i => new ItemCarrito(i.ProductoId, i.PresentacionId, i.Cantidad, i.Notas)).ToList()
            )).ToList()
        );

        try
        {
            var pedidos = await _pedidoService.CrearPedidoAsync(solicitud, usuarioId, cancellationToken);
            return Ok(pedidos.Select(MapearDto).ToList());
        }
        catch (ReglaDeNegocioException ex)
        {
            // Regla de negocio violada (carrito vacio, fuera de horario, ya no hay existencia...)
            // Nunca un 500 con detalle interno - un 400 con el mensaje pensado para el cliente.
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("mis")]
    public async Task<ActionResult<IReadOnlyList<PedidoDto>>> MisPedidos(CancellationToken cancellationToken)
    {
        var usuarioId = _userManager.GetUserId(User)!;
        var pedidos = await _pedidoService.ObtenerMisPedidosAsync(usuarioId, cancellationToken);
        return Ok(pedidos.Select(MapearDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoDto>> Obtener(int id, CancellationToken cancellationToken)
    {
        var usuarioId = _userManager.GetUserId(User)!;

        // El servicio ya filtra por UsuarioId == dueño: si el pedido es de otro usuario,
        // regresa null aqui y 404 (no 403) para no revelar ni que el pedido existe.
        var pedido = await _pedidoService.ObtenerPedidoAsync(id, usuarioId, cancellationToken);

        return pedido is null ? NotFound() : Ok(MapearDto(pedido));
    }

    private static PedidoDto MapearDto(Pedido pedido) => new(
        pedido.Id,
        pedido.CheckoutId,
        pedido.Vendedor.Nombre,
        pedido.PuntoEntrega.Nombre,
        pedido.FechaEntrega,
        pedido.HoraEntrega,
        pedido.DetalleEntrega,
        pedido.Total,
        pedido.Estado.ToString(),
        pedido.Pago?.Estado.ToString() ?? "Pendiente",
        pedido.Detalles.Select(d => new PedidoDetalleDto(d.Id, d.NombreProducto, d.PrecioUnitario, d.Cantidad, d.Notas)).ToList()
    );
}
