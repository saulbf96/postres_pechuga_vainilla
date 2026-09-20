using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Infrastructure.Services;

public class PerfilesService : IPerfilesService
{
    private readonly PechugaVainillaDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public PerfilesService(PechugaVainillaDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<PerfilUsuario>> ObtenerTodosAsync(CancellationToken cancellationToken)
    {
        var usuarios = await _userManager.Users.ToListAsync(cancellationToken);
        var asignaciones = await _context.AsignacionesVendedor
            .Include(a => a.Vendedor)
            .ToListAsync(cancellationToken);

        var resultado = new List<PerfilUsuario>();
        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            var asignacionesUsuario = asignaciones.Where(a => a.UsuarioId == usuario.Id).ToList();

            resultado.Add(new PerfilUsuario(usuario.Id, usuario.Nombre, usuario.Email!, usuario.PhoneNumber, roles.ToList(), asignacionesUsuario));
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

        await _userManager.AddToRoleAsync(usuario, nuevo.Rol);

        foreach (var asignacion in nuevo.Asignaciones)
        {
            await AgregarAsignacionAsync(usuario.Id, asignacion, cancellationToken);
        }

        return usuario.Id;
    }

    public async Task<AsignacionVendedor> AgregarAsignacionAsync(string usuarioId, NuevaAsignacion asignacion, CancellationToken cancellationToken)
    {
        var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == asignacion.VendedorId, cancellationToken)
            ?? throw new ReglaDeNegocioException("El vendedor no existe.");

        var nueva = new AsignacionVendedor
        {
            UsuarioId = usuarioId,
            Vendedor = vendedor,
            DiasSemana = asignacion.DiasSemana,
            HoraInicio = asignacion.HoraInicio,
            HoraFin = asignacion.HoraFin,
        };

        _context.AsignacionesVendedor.Add(nueva);
        await _context.SaveChangesAsync(cancellationToken);
        return nueva;
    }

    public async Task CambiarActivaAsignacionAsync(int asignacionId, bool activo, CancellationToken cancellationToken)
    {
        var asignacion = await _context.AsignacionesVendedor.FirstOrDefaultAsync(a => a.Id == asignacionId, cancellationToken)
            ?? throw new ReglaDeNegocioException("La asignación no existe.");

        asignacion.Activo = activo;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
