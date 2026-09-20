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

    public async Task<int?> ObtenerVendedorIdAsync(string usuarioId, CancellationToken cancellationToken)
    {
        return await _context.Vendedores
            .Where(v => v.UsuarioId == usuarioId)
            .Select(v => (int?)v.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
