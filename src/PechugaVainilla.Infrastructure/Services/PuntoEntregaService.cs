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

    public async Task<IReadOnlyList<PuntoEntrega>> ObtenerActivosAsync(int? categoriaId, CancellationToken cancellationToken)
    {
        var query = _context.PuntosEntrega
            .Include(pe => pe.Categorias).ThenInclude(pec => pec.Categoria)
            .Where(pe => pe.Activo);

        if (categoriaId.HasValue)
        {
            query = query.Where(pe => pe.Categorias.Any(pec => pec.CategoriaId == categoriaId.Value));
        }

        return await query.OrderBy(pe => pe.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PuntoEntrega>> ObtenerTodosAsync(CancellationToken cancellationToken)
    {
        return await _context.PuntosEntrega
            .Include(pe => pe.Categorias).ThenInclude(pec => pec.Categoria)
            .OrderBy(pe => pe.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<PuntoEntrega> CrearAsync(DatosPuntoEntrega datos, CancellationToken cancellationToken)
    {
        var punto = new PuntoEntrega
        {
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
        await AsignarCategoriasAsync(punto, datos.CategoriaIds, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return punto;
    }

    public async Task ActualizarAsync(int id, DatosPuntoEntrega datos, CancellationToken cancellationToken)
    {
        var punto = await ObtenerOFallarAsync(id, cancellationToken);

        punto.Nombre = datos.Nombre;
        punto.Tipo = datos.Tipo;
        punto.DiasSemana = datos.DiasSemana;
        punto.HoraInicio = datos.HoraInicio;
        punto.HoraFin = datos.HoraFin;
        punto.DiasAnticipacion = datos.DiasAnticipacion;
        punto.HoraLimitePedido = datos.HoraLimitePedido;
        punto.CostoEnvio = datos.CostoEnvio;

        punto.Categorias.Clear();
        await AsignarCategoriasAsync(punto, datos.CategoriaIds, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarActivoAsync(int id, bool activo, CancellationToken cancellationToken)
    {
        var punto = await ObtenerOFallarAsync(id, cancellationToken);
        punto.Activo = activo;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<PuntoEntrega> ObtenerOFallarAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.PuntosEntrega
            .Include(pe => pe.Categorias)
            .FirstOrDefaultAsync(pe => pe.Id == id, cancellationToken)
            ?? throw new ReglaDeNegocioException("El punto de entrega no existe.");
    }

    private async Task AsignarCategoriasAsync(PuntoEntrega punto, IReadOnlyList<int> categoriaIds, CancellationToken cancellationToken)
    {
        foreach (var categoriaId in categoriaIds)
        {
            var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == categoriaId, cancellationToken)
                ?? throw new ReglaDeNegocioException("Una de las categorias no existe.");

            punto.Categorias.Add(new PuntoEntregaCategoria { PuntoEntrega = punto, Categoria = categoria });
        }
    }
}
