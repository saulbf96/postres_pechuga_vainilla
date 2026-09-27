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
    decimal CostoEnvio,
    IReadOnlyList<int> CategoriaIds);

public interface IPuntoEntregaService
{
    // Publico: solo activos, opcionalmente filtrado por categoria (para el checkout).
    Task<IReadOnlyList<PuntoEntrega>> ObtenerActivosAsync(int? categoriaId, CancellationToken cancellationToken);

    // Panel: todos. Cualquier Administrador ve todo, sin filtro.
    Task<IReadOnlyList<PuntoEntrega>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task<PuntoEntrega> CrearAsync(DatosPuntoEntrega datos, CancellationToken cancellationToken);

    Task ActualizarAsync(int id, DatosPuntoEntrega datos, CancellationToken cancellationToken);

    Task CambiarActivoAsync(int id, bool activo, CancellationToken cancellationToken);
}
