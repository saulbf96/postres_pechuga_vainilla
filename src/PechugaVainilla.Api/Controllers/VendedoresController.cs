using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers;

// El controller no tiene logica de negocio: solo recibe la peticion, llama al servicio
// y convierte las entidades a DTOs antes de devolverlas (nunca entidades de EF directo).
[ApiController]
[Route("api/v1/vendedores")]
public class VendedoresController : ControllerBase
{
    private readonly IVendedorService _vendedorService;

    public VendedoresController(IVendedorService vendedorService)
    {
        _vendedorService = vendedorService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VendedorDto>>> Get(CancellationToken cancellationToken)
    {
        var vendedores = await _vendedorService.ObtenerActivosAsync(cancellationToken);

        var dtos = vendedores
            .Select(v => new VendedorDto(v.Id, v.Nombre, v.Slug, v.WhatsApp, v.Descripcion))
            .ToList();

        return Ok(dtos);
    }
}
