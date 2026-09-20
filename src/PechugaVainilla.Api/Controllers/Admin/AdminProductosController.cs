using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Api.Controllers.Admin;

// Panel: un Administrador ve/edita todo; un Vendedor solo lo de sus negocios asignados
// (ResolverVendedorIdsAsync se encarga de ese filtro en cada accion; una persona puede
// tener acceso a mas de un vendedor si ayuda a entregar de los dos lineas).
[ApiController]
[Route("api/v1/admin/productos")]
[Authorize(Roles = "Administrador,Vendedor")]
public class AdminProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly IAccesoVendedorService _accesoVendedorService;
    private readonly UserManager<Usuario> _userManager;

    public AdminProductosController(IProductoService productoService, IAccesoVendedorService accesoVendedorService, UserManager<Usuario> userManager)
    {
        _productoService = productoService;
        _accesoVendedorService = accesoVendedorService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminProductoDto>>> Get(CancellationToken cancellationToken)
    {
        var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
        var productos = await _productoService.ObtenerTodosAsync(vendedorIds, cancellationToken);
        return Ok(productos.Select(MapearDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AdminProductoDto>> Crear(CrearProductoRequest request, CancellationToken cancellationToken)
    {
        var vendedorId = await ResolverVendedorIdParaEscrituraAsync(request.VendedorId, cancellationToken);
        if (vendedorId is null)
        {
            return BadRequest(new { mensaje = "Indica a que vendedor pertenece el producto." });
        }

        try
        {
            var nuevo = new NuevoProducto(
                vendedorId.Value,
                request.Nombre,
                request.Descripcion,
                request.Alergenos,
                request.MaxPorDia,
                request.Presentaciones.Select(p => new PresentacionInput(p.Nombre, p.Precio)).ToList());

            var producto = await _productoService.CrearAsync(nuevo, cancellationToken);
            return Ok(MapearDto(producto));
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, EditarProductoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
            await _productoService.ActualizarAsync(
                id, vendedorIds,
                new EdicionProducto(request.Nombre, request.Descripcion, request.Alergenos, request.MaxPorDia),
                cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("{id:int}/activo")]
    public async Task<IActionResult> CambiarActivo(int id, CambiarActivoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
            await _productoService.CambiarActivoAsync(id, vendedorIds, request.Activo, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{productoId:int}/presentaciones")]
    public async Task<ActionResult<PresentacionAdminDto>> AgregarPresentacion(int productoId, PresentacionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
            var presentacion = await _productoService.AgregarPresentacionAsync(
                productoId, vendedorIds, new PresentacionInput(request.Nombre, request.Precio), cancellationToken);
            return Ok(new PresentacionAdminDto(presentacion.Id, presentacion.Nombre, presentacion.Precio, presentacion.Activo));
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("presentaciones/{presentacionId:int}")]
    public async Task<IActionResult> EditarPresentacion(int presentacionId, PresentacionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
            await _productoService.EditarPresentacionAsync(
                presentacionId, vendedorIds, new PresentacionInput(request.Nombre, request.Precio), cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("presentaciones/{presentacionId:int}/activo")]
    public async Task<IActionResult> CambiarActivaPresentacion(int presentacionId, CambiarActivoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
            await _productoService.CambiarActivaPresentacionAsync(presentacionId, vendedorIds, request.Activo, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // null = Administrador (sin filtro, ve todo). Con lista = Vendedor, solo esos negocios.
    private async Task<IReadOnlyList<int>?> ResolverVendedorIdsAsync(CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrador"))
        {
            return null;
        }

        var usuarioId = _userManager.GetUserId(User)!;
        return await _accesoVendedorService.ObtenerVendedorIdsAsync(usuarioId, cancellationToken);
    }

    // Al crear: un Administrador dice para cual vendedor es. Un Vendedor con acceso a uno solo
    // lo usa por default; si tiene acceso a varios, debe indicar cual (validado contra su lista).
    private async Task<int?> ResolverVendedorIdParaEscrituraAsync(int? vendedorIdSolicitado, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrador"))
        {
            return vendedorIdSolicitado;
        }

        var usuarioId = _userManager.GetUserId(User)!;
        var permitidos = await _accesoVendedorService.ObtenerVendedorIdsAsync(usuarioId, cancellationToken);

        if (vendedorIdSolicitado.HasValue && permitidos.Contains(vendedorIdSolicitado.Value))
        {
            return vendedorIdSolicitado;
        }

        return permitidos.Count == 1 ? permitidos[0] : null;
    }

    private static AdminProductoDto MapearDto(Producto producto) => new(
        producto.Id,
        producto.VendedorId,
        producto.Vendedor.Nombre,
        producto.Nombre,
        producto.Descripcion,
        producto.Alergenos,
        producto.FotoRuta,
        producto.MaxPorDia,
        producto.Activo,
        producto.Presentaciones.Select(p => new PresentacionAdminDto(p.Id, p.Nombre, p.Precio, p.Activo)).ToList());
}
