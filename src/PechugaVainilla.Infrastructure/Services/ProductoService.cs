using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Services;

// Implementacion real de IProductoService, usando EF Core contra SQL Server.
public class ProductoService : IProductoService
{
    private readonly PechugaVainillaDbContext _context;

    public ProductoService(PechugaVainillaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Producto>> ObtenerActivosAsync(int? vendedorId, CancellationToken cancellationToken)
    {
        // Include: trae tambien el Vendedor y las Presentaciones relacionadas en la misma consulta
        // (sin esto, EF Core no las carga automaticamente y saldrian nulas/vacias).
        var query = _context.Productos
            .Include(p => p.Vendedor)
            .Include(p => p.Presentaciones)
            .Where(p => p.Activo);

        // Filtro opcional: si mandan vendedorId, solo trae los productos de ese vendedor.
        if (vendedorId.HasValue)
        {
            query = query.Where(p => p.VendedorId == vendedorId.Value);
        }

        return await query
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }
}
