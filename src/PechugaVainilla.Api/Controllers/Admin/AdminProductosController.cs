using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers.Admin;

// Panel: cualquier Administrador ve y edita todos los productos, sin filtro.
[ApiController]
[Route("api/v1/admin/productos")]
[Authorize(Roles = "Administrador")]
public class AdminProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public AdminProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminProductoDto>>> Get(CancellationToken cancellationToken)
    {
        var productos = await _productoService.ObtenerTodosAsync(cancellationToken);
        return Ok(productos.Select(MapearDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AdminProductoDto>> Crear(CrearProductoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var nuevo = new NuevoProducto(
                request.CategoriaId,
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
            await _productoService.ActualizarAsync(
                id,
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
            await _productoService.CambiarActivoAsync(id, request.Activo, cancellationToken);
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
            var presentacion = await _productoService.AgregarPresentacionAsync(
                productoId, new PresentacionInput(request.Nombre, request.Precio), cancellationToken);
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
            await _productoService.EditarPresentacionAsync(
                presentacionId, new PresentacionInput(request.Nombre, request.Precio), cancellationToken);
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
            await _productoService.CambiarActivaPresentacionAsync(presentacionId, request.Activo, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private static AdminProductoDto MapearDto(Producto producto) => new(
        producto.Id,
        producto.CategoriaId,
        producto.Categoria.Nombre,
        producto.Nombre,
        producto.Descripcion,
        producto.Alergenos,
        producto.FotoRuta,
        producto.MaxPorDia,
        producto.Activo,
        producto.Presentaciones.Select(p => new PresentacionAdminDto(p.Id, p.Nombre, p.Precio, p.Activo)).ToList());
}
