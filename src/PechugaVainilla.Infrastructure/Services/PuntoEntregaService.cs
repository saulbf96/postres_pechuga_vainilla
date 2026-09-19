using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Services;

public class PuntoEntregaService : IPuntoEntregaService
{
    private readonly PechugaVainillaDbContext _context;

    public PuntoEntregaService(PechugaVainillaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PuntoEntrega>> ObtenerActivosAsync(int? vendedorId, CancellationToken cancellationToken)
    {
        var query = _context.PuntosEntrega.Where(pe => pe.Activo);

        if (vendedorId.HasValue)
        {
            query = query.Where(pe => pe.VendedorId == vendedorId.Value);
        }

        return await query.OrderBy(pe => pe.Nombre).ToListAsync(cancellationToken);
    }
}
