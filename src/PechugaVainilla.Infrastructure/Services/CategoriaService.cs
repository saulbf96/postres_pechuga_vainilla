using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Services;

// Implementacion real de ICategoriaService, usando EF Core contra SQL Server.
public class CategoriaService : ICategoriaService
{
    private readonly PechugaVainillaDbContext _context;

    public CategoriaService(PechugaVainillaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Categoria>> ObtenerActivasAsync(CancellationToken cancellationToken)
    {
        // Solo categorias activas, ordenadas por Orden para que el catalogo se vea consistente.
        return await _context.Categorias
            .Where(c => c.Activa)
            .OrderBy(c => c.Orden)
            .ToListAsync(cancellationToken);
    }
}
