using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Infrastructure.Services;

public class PerfilesService : IPerfilesService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly CifradoContrasena _cifradoContrasena;

    public PerfilesService(UserManager<Usuario> userManager, CifradoContrasena cifradoContrasena)
    {
        _userManager = userManager;
        _cifradoContrasena = cifradoContrasena;
    }

    public async Task<IReadOnlyList<PerfilUsuario>> ObtenerTodosAsync(CancellationToken cancellationToken)
    {
        var usuarios = await _userManager.Users.ToListAsync(cancellationToken);

        var resultado = new List<PerfilUsuario>();
        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            resultado.Add(new PerfilUsuario(usuario.Id, usuario.Nombre, usuario.Email!, usuario.PhoneNumber, roles.ToList(), usuario.DebeCambiarPassword, usuario.Activo));
        }

        return resultado.OrderBy(p => p.Nombre).ToList();
    }

    public async Task<UsuarioCreado> CrearUsuarioAsync(NuevoUsuarioPanel nuevo, CancellationToken cancellationToken)
    {
        var existente = await _userManager.FindByEmailAsync(nuevo.Email);
        if (existente is not null)
        {
            throw new ReglaDeNegocioException("Ya existe una cuenta con ese correo.");
        }

        var usuario = new Usuario
        {
            UserName = nuevo.Email,
            Email = nuevo.Email,
            PhoneNumber = nuevo.WhatsApp,
            Nombre = nuevo.Nombre,
            EmailConfirmed = true,
            DebeCambiarPassword = true,
        };

        var passwordTemporal = GenerarPasswordTemporal();

        var resultado = await _userManager.CreateAsync(usuario, passwordTemporal);
        if (!resultado.Succeeded)
        {
            throw new ReglaDeNegocioException(string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }

        await _cifradoContrasena.GuardarAsync(usuario.Id, passwordTemporal, cancellationToken);

        await _userManager.AddToRoleAsync(usuario, nuevo.Rol);

        return new UsuarioCreado(usuario.Id, passwordTemporal);
    }

    public async Task<bool> VerificarAdministradorAsync(string email, string password)
    {
        var admin = await _userManager.FindByEmailAsync(email);
        return admin is not null
            && admin.Activo
            && await _userManager.CheckPasswordAsync(admin, password)
            && await _userManager.IsInRoleAsync(admin, "Administrador");
    }

    public async Task<string?> ObtenerRolAsync(string usuarioId)
    {
        var usuario = await BuscarUsuarioAsync(usuarioId);
        return (await _userManager.GetRolesAsync(usuario)).FirstOrDefault();
    }

    public async Task EditarUsuarioAsync(string usuarioId, EdicionUsuarioPanel edicion, CancellationToken cancellationToken)
    {
        var usuario = await BuscarUsuarioAsync(usuarioId);

        var otroConEseCorreo = await _userManager.FindByEmailAsync(edicion.Email);
        if (otroConEseCorreo is not null && otroConEseCorreo.Id != usuario.Id)
        {
            throw new ReglaDeNegocioException("Ya existe otra cuenta con ese correo.");
        }

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        var cambiaRol = !rolesActuales.Contains(edicion.Rol);
        if (cambiaRol && rolesActuales.Contains("Administrador") && usuario.Activo && await ContarAdministradoresActivosAsync() <= 1)
        {
            throw new ReglaDeNegocioException("No se le puede quitar el rol al último administrador activo.");
        }

        usuario.Nombre = edicion.Nombre;
        usuario.PhoneNumber = edicion.WhatsApp;

        // El correo tambien es el usuario para iniciar sesion: se cambian los dos juntos.
        if (!string.Equals(usuario.Email, edicion.Email, StringComparison.OrdinalIgnoreCase))
        {
            usuario.Email = edicion.Email;
            usuario.UserName = edicion.Email;
        }

        var resultado = await _userManager.UpdateAsync(usuario);
        if (!resultado.Succeeded)
        {
            throw new ReglaDeNegocioException(string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }

        if (cambiaRol)
        {
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, edicion.Rol);
            // Cambiar el sello cierra sus sesiones, para que al volver a entrar ya tenga el rol nuevo.
            await _userManager.UpdateSecurityStampAsync(usuario);
        }
    }

    public async Task CambiarActivoAsync(string usuarioId, bool activo, string idAdminActual, CancellationToken cancellationToken)
    {
        var usuario = await BuscarUsuarioAsync(usuarioId);

        if (!activo)
        {
            if (usuario.Id == idAdminActual)
            {
                throw new ReglaDeNegocioException("No puedes eliminar tu propia cuenta.");
            }

            if (await _userManager.IsInRoleAsync(usuario, "Administrador") && usuario.Activo && await ContarAdministradoresActivosAsync() <= 1)
            {
                throw new ReglaDeNegocioException("No se puede eliminar al último administrador activo.");
            }
        }

        usuario.Activo = activo;
        await _userManager.UpdateAsync(usuario);

        if (!activo)
        {
            // Cierra las sesiones que tenga abiertas.
            await _userManager.UpdateSecurityStampAsync(usuario);
        }
    }

    private async Task<int> ContarAdministradoresActivosAsync() =>
        (await _userManager.GetUsersInRoleAsync("Administrador")).Count(u => u.Activo);

    public async Task<string> RestablecerPasswordAsync(string usuarioId, string idAdminActual, CancellationToken cancellationToken)
    {
        if (usuarioId == idAdminActual)
        {
            throw new ReglaDeNegocioException("No puedes restablecer tu propia contraseña. Pídeselo a otro administrador.");
        }

        var usuario = await BuscarUsuarioAsync(usuarioId);
        var passwordTemporal = GenerarPasswordTemporal();

        // RemovePassword + AddPassword tambien cambian el sello de seguridad (cierra sus sesiones).
        await _userManager.RemovePasswordAsync(usuario);
        var resultado = await _userManager.AddPasswordAsync(usuario, passwordTemporal);
        if (!resultado.Succeeded)
        {
            throw new ReglaDeNegocioException(string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }

        usuario.DebeCambiarPassword = true;
        await _userManager.UpdateAsync(usuario);

        await _cifradoContrasena.GuardarAsync(usuario.Id, passwordTemporal, cancellationToken);

        return passwordTemporal;
    }

    private async Task<Usuario> BuscarUsuarioAsync(string usuarioId) =>
        await _userManager.FindByIdAsync(usuarioId)
        ?? throw new ReglaDeNegocioException("El usuario no existe.");

    // Ej. "Temp4821Kq": cumple la politica (8+ caracteres, un numero, una mayuscula).
    // RandomNumberGenerator (no Random) porque es una contraseña: debe ser impredecible.
    private static string GenerarPasswordTemporal()
    {
        const string mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minusculas = "abcdefghijkmnpqrstuvwxyz";

        var numero = RandomNumberGenerator.GetInt32(1000, 10000);
        var mayuscula = mayusculas[RandomNumberGenerator.GetInt32(mayusculas.Length)];
        var minuscula = minusculas[RandomNumberGenerator.GetInt32(minusculas.Length)];

        return $"Temp{numero}{mayuscula}{minuscula}";
    }
}
