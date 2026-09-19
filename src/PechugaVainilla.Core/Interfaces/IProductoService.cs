using PechugaVainilla.Core.Entities;

namespace PechugaVainilla.Core.Interfaces;

// Interfaz: describe QUE se puede hacer, sin decir COMO.
// La implementacion real (con EF Core) vive en Infrastructure.
public interface IProductoService
{
    // vendedorId es opcional (int?) para poder filtrar el catalogo por vendedor o traer todos.
    Task<IReadOnlyList<Producto>> ObtenerActivosAsync(int? vendedorId, CancellationToken cancellationToken);
}
