using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly SignInManager<Usuario> _signInManager;

    public AuthController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("registro")]
    public async Task<ActionResult<UsuarioDto>> Registro(RegistroRequest request)
    {
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

    private async Task<UsuarioDto> ConstruirUsuarioDto(Usuario usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);
        return new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Email!, usuario.PhoneNumber, roles);
    }
}
