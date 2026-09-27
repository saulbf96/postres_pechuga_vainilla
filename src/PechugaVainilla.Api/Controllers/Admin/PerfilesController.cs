using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Api.Validacion;
using PechugaVainilla.Core.Interfaces;

namespace PechugaVainilla.Api.Controllers.Admin;

// Solo Administrador: dar de alta usuarios (cliente o administrador) y administrar sus contraseñas.
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
        if (request.Rol != "Administrador" && request.Rol != "Cliente")
        {
            return BadRequest(new { mensaje = "El rol debe ser Administrador o Cliente." });
        }

        // WhatsApp obligatorio para clientes (se les avisa de su pedido), opcional para administradores.
        var errores = ValidacionesUsuario.ValidarDatosBasicos(request.Nombre, request.Email, request.WhatsApp, whatsAppRequerido: request.Rol == "Cliente");
        if (errores.Count > 0)
        {
            return BadRequest(new { mensaje = string.Join(" ", errores) });
        }

        try
        {
            var creado = await _perfilesService.CrearUsuarioAsync(
                new NuevoUsuarioPanel(request.Nombre, request.Email, request.WhatsApp, request.Rol),
                cancellationToken);
            return Ok(new { id = creado.Id, passwordTemporal = creado.PasswordTemporal });
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // Restablecer: para quien olvido su contraseña. Regresa una temporal para pasarsela al usuario.
    [HttpPost("{id}/restablecer-password")]
    public async Task<IActionResult> RestablecerPassword(string id, AutorizacionAdminRequest request, CancellationToken cancellationToken)
    {
        if (!await _perfilesService.VerificarAdministradorAsync(request.EmailAdmin, request.PasswordAdmin))
        {
            return BadRequest(new { mensaje = "El correo o la contraseña del administrador no son correctos." });
        }

        try
        {
            var idAdminActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var passwordTemporal = await _perfilesService.RestablecerPasswordAsync(id, idAdminActual, cancellationToken);
            return Ok(new { passwordTemporal });
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(string id, EditarUsuarioRequest request, CancellationToken cancellationToken)
    {
        if (request.Rol != "Administrador" && request.Rol != "Cliente")
        {
            return BadRequest(new { mensaje = "El rol debe ser Administrador o Cliente." });
        }

        var errores = ValidacionesUsuario.ValidarDatosBasicos(request.Nombre, request.Email, request.WhatsApp, whatsAppRequerido: request.Rol == "Cliente");
        if (errores.Count > 0)
        {
            return BadRequest(new { mensaje = string.Join(" ", errores) });
        }

        try
        {
            // Cambiar el rol es delicado (da o quita acceso al Panel): pide datos de un administrador.
            var rolActual = await _perfilesService.ObtenerRolAsync(id);
            if (rolActual != request.Rol
                && !await _perfilesService.VerificarAdministradorAsync(request.EmailAdmin ?? "", request.PasswordAdmin ?? ""))
            {
                return BadRequest(new { mensaje = "Para cambiar el rol, escribe correctamente el correo y la contraseña de un administrador." });
            }

            await _perfilesService.EditarUsuarioAsync(
                id,
                new EdicionUsuarioPanel(request.Nombre, request.Email, request.WhatsApp, request.Rol),
                cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id}/desactivar")]
    public Task<IActionResult> Desactivar(string id, AutorizacionAdminRequest request, CancellationToken cancellationToken) =>
        CambiarActivo(id, activo: false, request, cancellationToken);

    [HttpPost("{id}/reactivar")]
    public Task<IActionResult> Reactivar(string id, AutorizacionAdminRequest request, CancellationToken cancellationToken) =>
        CambiarActivo(id, activo: true, request, cancellationToken);

    private async Task<IActionResult> CambiarActivo(string id, bool activo, AutorizacionAdminRequest request, CancellationToken cancellationToken)
    {
        if (!await _perfilesService.VerificarAdministradorAsync(request.EmailAdmin, request.PasswordAdmin))
        {
            return BadRequest(new { mensaje = "El correo o la contraseña del administrador no son correctos." });
        }

        try
        {
            var idAdminActual = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            await _perfilesService.CambiarActivoAsync(id, activo, idAdminActual, cancellationToken);
            return NoContent();
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
        perfil.Roles.ToList(),
        perfil.DebeCambiarPassword,
        perfil.Activo);
}
