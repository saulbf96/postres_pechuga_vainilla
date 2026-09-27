using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Api.Validacion;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers.Admin;

// Solo Administrador: dar de alta al otro administrador. Con solo 2 roles fijos
// (Administrador/Cliente) ya no hace falta asignar vendedores ni horarios.
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
        var errores = ValidacionesUsuario.ValidarDatosBasicos(request.Nombre, request.Email, request.WhatsApp, whatsAppRequerido: false);
        if (errores.Count > 0)
        {
            return BadRequest(new { mensaje = string.Join(" ", errores) });
        }

        if (request.Rol != "Administrador")
        {
            return BadRequest(new { mensaje = "El rol debe ser Administrador." });
        }

        try
        {
            var id = await _perfilesService.CrearUsuarioAsync(
                new NuevoUsuarioPanel(request.Nombre, request.Email, request.WhatsApp, request.Password, request.Rol),
                cancellationToken);
            return Ok(new { id });
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private static PerfilDto MapearDto(PerfilUsuario perfil) => new(
        perfil.Id,
        perfil.Nombre,
        perfil.Email,
        perfil.WhatsApp,
        perfil.Roles.ToList());
}
