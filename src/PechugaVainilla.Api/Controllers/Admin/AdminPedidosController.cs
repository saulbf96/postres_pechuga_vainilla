using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/pedidos")]
[Authorize(Roles = "Administrador")]
public class AdminPedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public AdminPedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPedidoDto>>> Get(CancellationToken cancellationToken)
    {
        var pedidos = await _pedidoService.ObtenerTodosAsync(cancellationToken);
        return Ok(pedidos.Select(MapearDto).ToList());
    }

    // El "avisito" dentro del panel: cuantos pedidos Nuevo hay ahora mismo.
    [HttpGet("nuevos/contar")]
    public async Task<ActionResult<int>> ContarNuevos(CancellationToken cancellationToken)
    {
        var cantidad = await _pedidoService.ContarNuevosAsync(cancellationToken);
        return Ok(cantidad);
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoPedidoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _pedidoService.CambiarEstadoAsync(id, request.Estado, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("{id:int}/pago")]
    public async Task<IActionResult> CambiarEstadoPago(int id, CambiarEstadoPagoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _pedidoService.CambiarEstadoPagoAsync(id, request.Estado, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private static AdminPedidoDto MapearDto(Pedido pedido) => new(
        pedido.Id,
        pedido.NombreCliente,
        pedido.WhatsApp,
        pedido.PuntoEntrega.Nombre,
        pedido.FechaEntrega,
        pedido.HoraEntrega,
        pedido.DetalleEntrega,
        pedido.Total,
        pedido.Estado.ToString(),
        pedido.Pago?.Estado.ToString() ?? "Pendiente",
        pedido.Detalles.Select(d => new PedidoDetalleDto(d.Id, d.NombreProducto, d.CategoriaId, d.PrecioUnitario, d.Cantidad, d.Notas)).ToList());
}
