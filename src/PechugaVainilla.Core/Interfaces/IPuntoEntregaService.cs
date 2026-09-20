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

    // Panel: todos.
    Task<IReadOnlyList<PuntoEntrega>> ObtenerTodosAsync(int? vendedorId, CancellationToken cancellationToken);

    Task<PuntoEntrega> CrearAsync(int vendedorId, DatosPuntoEntrega datos, CancellationToken cancellationToken);

    Task ActualizarAsync(int id, int? vendedorIdPermitido, DatosPuntoEntrega datos, CancellationToken cancellationToken);

    Task CambiarActivoAsync(int id, int? vendedorIdPermitido, bool activo, CancellationToken cancellationToken);
}
