using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Services;

// Implementacion real de IVendedorService, usando EF Core contra SQL Server.
public class VendedorService : IVendedorService
{
    private readonly PechugaVainillaDbContext _context;

    public VendedorService(PechugaVainillaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Vendedor>> ObtenerActivosAsync(CancellationToken cancellationToken)
    {
        // Solo vendedores activos, ordenados por nombre para que el catalogo se vea consistente.
        return await _context.Vendedores
            .Where(v => v.Activo)
            .OrderBy(v => v.Nombre)
            .ToListAsync(cancellationToken);
    }
}
