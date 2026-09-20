using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers.Admin;

// Solo Administrador: dar de alta gente y decidir su rol y a que negocio(s) ayuda a entregar.
// Un Vendedor normal no puede crear cuentas ni cambiar roles de nadie.
[ApiController]
[Route("api/v1/admin/perfiles")]
[Authorize(Roles = "Administrador")]
public class PerfilesController : ControllerBase
{
    private readonly IPerfilesService _perfilesService;

    public PerfilesController(IPerfilesService perfilesService)
    {
        _perfilesService = perfilesService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PerfilDto>>> Get(CancellationToken cancellationToken)
    {
        var perfiles = await _perfilesService.ObtenerTodosAsync(cancellationToken);
        return Ok(perfiles.Select(MapearDto).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(CrearUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (request.Rol is not ("Administrador" or "Vendedor"))
        {
            return BadRequest(new { mensaje = "El rol debe ser Administrador o Vendedor." });
        }

        var asignaciones = new List<NuevaAsignacion>();
        foreach (var asignacionRequest in request.Asignaciones)
        {
            if (!TryMapearAsignacion(asignacionRequest, out var asignacion, out var error))
            {
                return BadRequest(new { mensaje = error });
            }
            asignaciones.Add(asignacion);
        }

        try
        {
            var id = await _perfilesService.CrearUsuarioAsync(
                new NuevoUsuarioPanel(request.Nombre, request.Email, request.WhatsApp, request.Password, request.Rol, asignaciones),
                cancellationToken);
            return Ok(new { id });
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{usuarioId}/asignaciones")]
    public async Task<ActionResult<AsignacionDto>> AgregarAsignacion(string usuarioId, NuevaAsignacionRequest request, CancellationToken cancellationToken)
    {
        if (!TryMapearAsignacion(request, out var asignacion, out var error))
        {
            return BadRequest(new { mensaje = error });
        }

        try
        {
            var creada = await _perfilesService.AgregarAsignacionAsync(usuarioId, asignacion, cancellationToken);
            return Ok(MapearAsignacionDto(creada));
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("asignaciones/{asignacionId:int}/activo")]
    public async Task<IActionResult> CambiarActivaAsignacion(int asignacionId, CambiarActivoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _perfilesService.CambiarActivaAsignacionAsync(asignacionId, request.Activo, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private static bool TryMapearAsignacion(NuevaAsignacionRequest request, out NuevaAsignacion asignacion, out string? error)
    {
        asignacion = null!;
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

        if (!TimeOnly.TryParse(request.HoraInicio, out var horaInicio) || !TimeOnly.TryParse(request.HoraFin, out var horaFin))
        {
            error = "Alguna de las horas no tiene un formato válido (HH:mm).";
            return false;
        }

        asignacion = new NuevaAsignacion(request.VendedorId, dias, horaInicio, horaFin);
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

    private static AsignacionDto MapearAsignacionDto(AsignacionVendedor a) => new(
        a.Id, a.VendedorId, a.Vendedor.Nombre, DesglosarDias(a.DiasSemana), a.HoraInicio.ToString("HH:mm"), a.HoraFin.ToString("HH:mm"), a.Activo);

    private static PerfilDto MapearDto(PerfilUsuario perfil) => new(
        perfil.Id,
        perfil.Nombre,
        perfil.Email,
        perfil.WhatsApp,
        perfil.Roles.ToList(),
        perfil.Asignaciones.Select(MapearAsignacionDto).ToList());
}
