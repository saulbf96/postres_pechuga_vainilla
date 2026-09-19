using PechugaVainilla.Core.Entities;

namespace PechugaVainilla.Core.Interfaces;

public interface IPuntoEntregaService
{
    Task<IReadOnlyList<PuntoEntrega>> ObtenerActivosAsync(int? vendedorId, CancellationToken cancellationToken);
}
