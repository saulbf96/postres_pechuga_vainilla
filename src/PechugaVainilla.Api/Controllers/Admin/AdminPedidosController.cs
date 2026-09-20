using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/pedidos")]
[Authorize(Roles = "Administrador,Vendedor")]
public class AdminPedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;
    private readonly IAccesoVendedorService _accesoVendedorService;
    private readonly UserManager<Usuario> _userManager;

    public AdminPedidosController(IPedidoService pedidoService, IAccesoVendedorService accesoVendedorService, UserManager<Usuario> userManager)
    {
        _pedidoService = pedidoService;
        _accesoVendedorService = accesoVendedorService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPedidoDto>>> Get(CancellationToken cancellationToken)
    {
        var vendedorId = await ResolverVendedorIdAsync(cancellationToken);
        var pedidos = await _pedidoService.ObtenerPorVendedorAsync(vendedorId, cancellationToken);
        return Ok(pedidos.Select(MapearDto).ToList());
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoPedidoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vendedorIdPermitido = await ResolverVendedorIdAsync(cancellationToken);
            await _pedidoService.CambiarEstadoAsync(id, vendedorIdPermitido, request.Estado, cancellationToken);
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
            var vendedorIdPermitido = await ResolverVendedorIdAsync(cancellationToken);
            await _pedidoService.CambiarEstadoPagoAsync(id, vendedorIdPermitido, request.Estado, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private async Task<int?> ResolverVendedorIdAsync(CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrador"))
        {
            return null;
        }

        var usuarioId = _userManager.GetUserId(User)!;
        return await _accesoVendedorService.ObtenerVendedorIdAsync(usuarioId, cancellationToken);
    }

    private static AdminPedidoDto MapearDto(Pedido pedido) => new(
        pedido.Id,
        pedido.CheckoutId,
        pedido.Vendedor.Nombre,
        pedido.NombreCliente,
        pedido.WhatsApp,
        pedido.PuntoEntrega.Nombre,
        pedido.FechaEntrega,
        pedido.HoraEntrega,
        pedido.DetalleEntrega,
        pedido.Total,
        pedido.Estado.ToString(),
        pedido.Pago?.Estado.ToString() ?? "Pendiente",
        pedido.Detalles.Select(d => new PedidoDetalleDto(d.Id, d.NombreProducto, d.PrecioUnitario, d.Cantidad, d.Notas)).ToList());
}
