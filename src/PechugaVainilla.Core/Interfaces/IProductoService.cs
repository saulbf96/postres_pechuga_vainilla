using PechugaVainilla.Core.Entities;

namespace PechugaVainilla.Core.Interfaces;

public record PresentacionInput(string Nombre, decimal Precio);

public record NuevoProducto(
    int CategoriaId,
    string Nombre,
    string? Descripcion,
    string? Alergenos,
    int? MaxPorDia,
    IReadOnlyList<PresentacionInput> Presentaciones);

public record EdicionProducto(string Nombre, string? Descripcion, string? Alergenos, int? MaxPorDia);

public interface IProductoService
{
    // Publico (catalogo): solo activos, opcionalmente filtrado por categoria.
    Task<IReadOnlyList<Producto>> ObtenerActivosAsync(int? categoriaId, CancellationToken cancellationToken);

    // Panel: todos (activos e inactivos). Cualquier Administrador ve todo, sin filtro.
    Task<IReadOnlyList<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task<Producto> CrearAsync(NuevoProducto nuevo, CancellationToken cancellationToken);

    Task ActualizarAsync(int id, EdicionProducto edicion, CancellationToken cancellationToken);

    Task CambiarActivoAsync(int id, bool activo, CancellationToken cancellationToken);

    // Gestion de presentaciones (tamaños/precios) de un producto ya existente.
    Task<Presentacion> AgregarPresentacionAsync(int productoId, PresentacionInput input, CancellationToken cancellationToken);

    Task EditarPresentacionAsync(int presentacionId, PresentacionInput input, CancellationToken cancellationToken);

    Task CambiarActivaPresentacionAsync(int presentacionId, bool activo, CancellationToken cancellationToken);
}
