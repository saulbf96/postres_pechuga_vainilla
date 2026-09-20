using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Services;

public class AccesoVendedorService : IAccesoVendedorService
{
    private readonly PechugaVainillaDbContext _context;

    public AccesoVendedorService(PechugaVainillaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<int>> ObtenerVendedorIdsAsync(string usuarioId, CancellationToken cancellationToken)
    {
        // Acceso por ser dueño del negocio (Vendedor.UsuarioId)...
        var comoDueño = _context.Vendedores
            .Where(v => v.UsuarioId == usuarioId)
            .Select(v => v.Id);

        // ...mas acceso por estar asignado a entregar ahi (empleado, activo).
        var comoAsignado = _context.AsignacionesVendedor
            .Where(a => a.UsuarioId == usuarioId && a.Activo)
            .Select(a => a.VendedorId);

        return await comoDueño.Union(comoAsignado).ToListAsync(cancellationToken);
    }
}
