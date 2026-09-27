using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/puntos-entrega")]
[Authorize(Roles = "Administrador")]
public class AdminPuntosEntregaController : ControllerBase
{
    private readonly IPuntoEntregaService _puntoEntregaService;

    public AdminPuntosEntregaController(IPuntoEntregaService puntoEntregaService)
    {
        _puntoEntregaService = puntoEntregaService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPuntoEntregaDto>>> Get(CancellationToken cancellationToken)
    {
        var puntos = await _puntoEntregaService.ObtenerTodosAsync(cancellationToken);
        return Ok(puntos.Select(MapearDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AdminPuntoEntregaDto>> Crear(GuardarPuntoEntregaRequest request, CancellationToken cancellationToken)
    {
        if (!TryMapearDatos(request, out var datos, out var error))
        {
            return BadRequest(new { mensaje = error });
        }

        try
        {
            var punto = await _puntoEntregaService.CrearAsync(datos, cancellationToken);
            return Ok(MapearDto(punto));
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, GuardarPuntoEntregaRequest request, CancellationToken cancellationToken)
    {
        if (!TryMapearDatos(request, out var datos, out var error))
        {
            return BadRequest(new { mensaje = error });
        }

        try
        {
            await _puntoEntregaService.ActualizarAsync(id, datos, cancellationToken);
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
            await _puntoEntregaService.CambiarActivoAsync(id, request.Activo, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private static bool TryMapearDatos(GuardarPuntoEntregaRequest request, out DatosPuntoEntrega datos, out string? error)
    {
        datos = null!;
        error = null;

        var dias = DiasSemana.Ninguno;
        foreach (var nombreDia in request.DiasSemana)
        {
            if (!Enum.TryParse<DiasSemana>(nombreDia, ignoreCase: true, out var dia))
            {
                error = $"'{nombreDia}' no es un día válido.";
                return false;
            }
            dias |= dia;
        }

        if (!TimeOnly.TryParse(request.HoraInicio, out var horaInicio) ||
            !TimeOnly.TryParse(request.HoraFin, out var horaFin) ||
            !TimeOnly.TryParse(request.HoraLimitePedido, out var horaLimite))
        {
            error = "Alguna de las horas no tiene un formato válido (HH:mm).";
            return false;
        }

        datos = new DatosPuntoEntrega(
            request.Nombre, request.Tipo, dias, horaInicio, horaFin,
            request.DiasAnticipacion, horaLimite, request.CostoEnvio, request.CategoriaIds);
        return true;
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

    private static AdminPuntoEntregaDto MapearDto(PuntoEntrega punto) => new(
        punto.Id,
        punto.Nombre,
        punto.Tipo.ToString(),
        DesglosarDias(punto.DiasSemana),
        punto.HoraInicio.ToString("HH:mm"),
        punto.HoraFin.ToString("HH:mm"),
        punto.DiasAnticipacion,
        punto.HoraLimitePedido.ToString("HH:mm"),
        punto.CostoEnvio,
        punto.Activo,
        punto.Categorias.Select(c => c.CategoriaId).ToList());
}
