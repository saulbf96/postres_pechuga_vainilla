using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers;

// El controller no tiene logica de negocio: solo recibe la peticion, llama al servicio
// y convierte las entidades a DTOs antes de devolverlas (nunca entidades de EF directo).
[ApiController]
[Route("api/v1/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Get(CancellationToken cancellationToken)
    {
        var categorias = await _categoriaService.ObtenerActivasAsync(cancellationToken);

        var dtos = categorias
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.Slug, c.Descripcion, c.Color, c.ColorSuave, c.FotoRuta, c.Orden))
            .ToList();

        return Ok(dtos);
    }
}
