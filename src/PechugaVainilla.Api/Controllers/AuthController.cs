using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Api.Validacion;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;
    private readonly CifradoContrasena _cifradoContrasena;

    public AuthController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, CifradoContrasena cifradoContrasena)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _cifradoContrasena = cifradoContrasena;
    }

    [HttpPost("registro")]
    public async Task<ActionResult<UsuarioDto>> Registro(RegistroRequest request)
    {
        var errores = ValidacionesUsuario.ValidarDatosBasicos(request.Nombre, request.Email, request.WhatsApp, whatsAppRequerido: true);
        if (errores.Count > 0)
        {
            return BadRequest(new { errores });
        }

        var usuario = new Usuario
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.WhatsApp,
            Nombre = request.Nombre,
        };

        // CreateAsync ya valida la politica de contraseñas configurada en Program.cs
        // y hashea la contraseña - nunca la guardamos en texto plano.
        var resultado = await _userManager.CreateAsync(usuario, request.Password);
        if (!resultado.Succeeded)
        {
            return BadRequest(new { errores = resultado.Errors.Select(e => e.Description) });
        }

        // Despues de CreateAsync: el usuario ya existe en la tabla y se le puede hacer el UPDATE.
        await _cifradoContrasena.GuardarAsync(usuario.Id, request.Password);

        await _userManager.AddToRoleAsync(usuario, "Cliente");
        await _signInManager.SignInAsync(usuario, isPersistent: true);

        return Ok(await ConstruirUsuarioDto(usuario));
    }

    [HttpPost("login")]
    public async Task<ActionResult<UsuarioDto>> Login(LoginRequest request)
    {
        var usuario = await _userManager.FindByEmailAsync(request.Email);
        if (usuario is null)
        {
            // Mismo mensaje si el correo no existe o si la contraseña esta mal -
            // no le decimos a un atacante cual de las dos cosas fallo.
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        }

        var resultado = await _signInManager.PasswordSignInAsync(usuario, request.Password, isPersistent: true, lockoutOnFailure: true);

        if (resultado.IsLockedOut)
        {
            return Unauthorized(new { mensaje = "Cuenta bloqueada temporalmente por demasiados intentos fallidos." });
        }

        if (!resultado.Succeeded)
        {
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
        }

        return Ok(await ConstruirUsuarioDto(usuario));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("yo")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> Yo()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return Unauthorized();
        }

        return Ok(await ConstruirUsuarioDto(usuario));
    }

    // Solo para quien tiene la contraseña expirada/temporal: acaba de demostrar quien es al
    // iniciar sesion, asi que aqui solo pide la nueva.
    [HttpPost("cambiar-password-expirada")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> CambiarPasswordExpirada(CambiarPasswordExpiradaRequest request)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario is null)
        {
            return Unauthorized();
        }

        if (!usuario.DebeCambiarPassword)
        {
            return BadRequest(new { mensaje = "Tu contraseña no necesita cambiarse." });
        }

        if (await _userManager.CheckPasswordAsync(usuario, request.PasswordNueva))
        {
            return BadRequest(new { mensaje = "La contraseña nueva debe ser distinta a la anterior." });
        }

        // Valida la nueva contra la politica antes de quitar la vieja, para no dejarlo sin contraseña.
        var errores = new List<string>();
        foreach (var validador in _userManager.PasswordValidators)
        {
            var validacion = await validador.ValidateAsync(_userManager, usuario, request.PasswordNueva);
            errores.AddRange(validacion.Errors.Select(e => e.Description));
        }
        if (errores.Count > 0)
        {
            return BadRequest(new { mensaje = string.Join(" ", errores) });
        }

        await _userManager.RemovePasswordAsync(usuario);
        var resultado = await _userManager.AddPasswordAsync(usuario, request.PasswordNueva);
        if (!resultado.Succeeded)
        {
            return BadRequest(new { mensaje = string.Join(" ", resultado.Errors.Select(e => e.Description)) });
        }

        usuario.DebeCambiarPassword = false;
        await _userManager.UpdateAsync(usuario);
        await _cifradoContrasena.GuardarAsync(usuario.Id, request.PasswordNueva);

        // Cambiar la contraseña cambia el sello de seguridad: se renueva la cookie para no sacarlo.
        await _signInManager.RefreshSignInAsync(usuario);

        return Ok(await ConstruirUsuarioDto(usuario));
    }

    private async Task<UsuarioDto> ConstruirUsuarioDto(Usuario usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);
        return new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Email!, usuario.PhoneNumber, roles, usuario.DebeCambiarPassword);
    }
}
