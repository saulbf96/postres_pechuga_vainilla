using PechugaVainilla.Core.Entities;

namespace PechugaVainilla.Core.Interfaces;

// Interfaz: describe QUE se puede hacer, sin decir COMO.
// La implementacion real (con EF Core) vive en Infrastructure.
public interface IVendedorService
{
    Task<IReadOnlyList<Vendedor>> ObtenerActivosAsync(CancellationToken cancellationToken);
}
