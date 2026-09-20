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
        var query = _context.PuntosEntrega.Include(pe => pe.Vendedor).Where(pe => pe.Activo);

        if (vendedorId.HasValue)
        {
            query = query.Where(pe => pe.VendedorId == vendedorId.Value);
        }

        return await query.OrderBy(pe => pe.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PuntoEntrega>> ObtenerTodosAsync(int? vendedorId, CancellationToken cancellationToken)
    {
        var query = _context.PuntosEntrega.Include(pe => pe.Vendedor).AsQueryable();

        if (vendedorId.HasValue)
        {
            query = query.Where(pe => pe.VendedorId == vendedorId.Value);
        }

        return await query.OrderBy(pe => pe.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<PuntoEntrega> CrearAsync(int vendedorId, DatosPuntoEntrega datos, CancellationToken cancellationToken)
    {
        var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == vendedorId, cancellationToken)
            ?? throw new ReglaDeNegocioException("El vendedor no existe.");

        var punto = new PuntoEntrega
        {
            Vendedor = vendedor,
            Nombre = datos.Nombre,
            Tipo = datos.Tipo,
            DiasSemana = datos.DiasSemana,
            HoraInicio = datos.HoraInicio,
            HoraFin = datos.HoraFin,
            DiasAnticipacion = datos.DiasAnticipacion,
            HoraLimitePedido = datos.HoraLimitePedido,
            CostoEnvio = datos.CostoEnvio,
        };

        _context.PuntosEntrega.Add(punto);
        await _context.SaveChangesAsync(cancellationToken);
        return punto;
    }

    public async Task ActualizarAsync(int id, int? vendedorIdPermitido, DatosPuntoEntrega datos, CancellationToken cancellationToken)
    {
        var punto = await ObtenerConPermisoAsync(id, vendedorIdPermitido, cancellationToken);

        punto.Nombre = datos.Nombre;
        punto.Tipo = datos.Tipo;
        punto.DiasSemana = datos.DiasSemana;
        punto.HoraInicio = datos.HoraInicio;
        punto.HoraFin = datos.HoraFin;
        punto.DiasAnticipacion = datos.DiasAnticipacion;
        punto.HoraLimitePedido = datos.HoraLimitePedido;
        punto.CostoEnvio = datos.CostoEnvio;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarActivoAsync(int id, int? vendedorIdPermitido, bool activo, CancellationToken cancellationToken)
    {
        var punto = await ObtenerConPermisoAsync(id, vendedorIdPermitido, cancellationToken);
        punto.Activo = activo;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<PuntoEntrega> ObtenerConPermisoAsync(int id, int? vendedorIdPermitido, CancellationToken cancellationToken)
    {
        var punto = await _context.PuntosEntrega.FirstOrDefaultAsync(pe => pe.Id == id, cancellationToken)
            ?? throw new ReglaDeNegocioException("El punto de entrega no existe.");

        if (vendedorIdPermitido.HasValue && punto.VendedorId != vendedorIdPermitido.Value)
        {
            throw new ReglaDeNegocioException("No tienes permiso sobre este punto de entrega.");
        }

        return punto;
    }
}
