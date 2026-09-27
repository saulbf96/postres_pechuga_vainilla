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
            resultado.Add(new PerfilUsuario(usuario.Id, usuario.Nombre, usuario.Email!, usuario.PhoneNumber, roles.ToList()));
        }

        return resultado.OrderBy(p => p.Nombre).ToList();
    }

    public async Task<string> CrearUsuarioAsync(NuevoUsuarioPanel nuevo, CancellationToken cancellationToken)
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
        };

        var resultado = await _userManager.CreateAsync(usuario, nuevo.Password);
        if (!resultado.Succeeded)
        {
            throw new ReglaDeNegocioException(string.Join(" ", resultado.Errors.Select(e => e.Description)));
        }

        await _cifradoContrasena.GuardarAsync(usuario.Id, nuevo.Password, cancellationToken);

        await _userManager.AddToRoleAsync(usuario, nuevo.Rol);

        return usuario.Id;
    }
}
