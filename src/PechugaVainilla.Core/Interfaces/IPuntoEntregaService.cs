using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Core.Interfaces;

public record DatosPuntoEntrega(
    string Nombre,
    TipoPuntoEntrega Tipo,
    DiasSemana DiasSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    int DiasAnticipacion,
    TimeOnly HoraLimitePedido,
    decimal CostoEnvio);

public interface IPuntoEntregaService
{
    // Publico: solo activos.
    Task<IReadOnlyList<PuntoEntrega>> ObtenerActivosAsync(int? vendedorId, CancellationToken cancellationToken);

    // Panel: todos. null = Administrador (sin filtro).
    Task<IReadOnlyList<PuntoEntrega>> ObtenerTodosAsync(IReadOnlyList<int>? vendedorIdsPermitidos, CancellationToken cancellationToken);

    Task<PuntoEntrega> CrearAsync(int vendedorId, DatosPuntoEntrega datos, CancellationToken cancellationToken);

    Task ActualizarAsync(int id, IReadOnlyList<int>? vendedorIdsPermitidos, DatosPuntoEntrega datos, CancellationToken cancellationToken);

    Task CambiarActivoAsync(int id, IReadOnlyList<int>? vendedorIdsPermitidos, bool activo, CancellationToken cancellationToken);
}
