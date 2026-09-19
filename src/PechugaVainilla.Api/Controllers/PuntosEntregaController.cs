using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Enums;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers;

[ApiController]
[Route("api/v1/puntos-entrega")]
public class PuntosEntregaController : ControllerBase
{
    private readonly IPuntoEntregaService _puntoEntregaService;

    public PuntosEntregaController(IPuntoEntregaService puntoEntregaService)
    {
        _puntoEntregaService = puntoEntregaService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PuntoEntregaDto>>> Get([FromQuery] int? vendedorId, CancellationToken cancellationToken)
    {
        var puntos = await _puntoEntregaService.ObtenerActivosAsync(vendedorId, cancellationToken);

        var dtos = puntos.Select(pe => new PuntoEntregaDto(
            pe.Id,
            pe.VendedorId,
            pe.Nombre,
            pe.Tipo.ToString(),
            DesglosarDias(pe.DiasSemana),
            pe.HoraInicio.ToString("HH:mm"),
            pe.HoraFin.ToString("HH:mm"),
            pe.DiasAnticipacion,
            pe.HoraLimitePedido.ToString("HH:mm"),
            pe.CostoEnvio
        )).ToList();

        return Ok(dtos);
    }

    private static List<string> DesglosarDias(DiasSemana dias)
    {
        var resultado = new List<string>();
        foreach (var dia in Enum.GetValues<DiasSemana>())
        {
            if (dia != DiasSemana.Ninguno && dias.HasFlag(dia))
            {
                resultado.Add(dia.ToString());
            }
        }
        return resultado;
    }
}
