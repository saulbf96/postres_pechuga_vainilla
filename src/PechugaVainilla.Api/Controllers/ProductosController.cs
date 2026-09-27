using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers;

[ApiController]
[Route("api/v1/productos")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    // [FromQuery] categoriaId: filtro opcional, ej. GET /api/v1/productos?categoriaId=1
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> Get([FromQuery] int? categoriaId, CancellationToken cancellationToken)
    {
        var productos = await _productoService.ObtenerActivosAsync(categoriaId, cancellationToken);

        var dtos = productos.Select(p => new ProductoDto(
            p.Id,
            p.Nombre,
            p.Descripcion,
            p.Alergenos,
            p.FotoRuta,
            p.CategoriaId,
            p.Categoria.Nombre,
            p.Presentaciones
                .Where(pr => pr.Activo)
                .Select(pr => new PresentacionDto(pr.Id, pr.Nombre, pr.Precio))
                .ToList()
        )).ToList();

        return Ok(dtos);
    }
}
