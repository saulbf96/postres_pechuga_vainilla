using PechugaVainilla.Core.Entities;

namespace PechugaVainilla.Core.Interfaces;

public record PresentacionInput(string Nombre, decimal Precio);

public record NuevoProducto(
    int VendedorId,
    string Nombre,
    string? Descripcion,
    string? Alergenos,
    int? MaxPorDia,
    IReadOnlyList<PresentacionInput> Presentaciones);

public record EdicionProducto(string Nombre, string? Descripcion, string? Alergenos, int? MaxPorDia);

public interface IProductoService
{
    // Publico (catalogo): solo activos, opcionalmente filtrado por vendedor.
    Task<IReadOnlyList<Producto>> ObtenerActivosAsync(int? vendedorId, CancellationToken cancellationToken);

    // Panel: todos (activos e inactivos), para poder reactivarlos.
    // vendedorIdsPermitidos null = Administrador (sin filtro). Lista = restringido a esos vendedores.
    Task<IReadOnlyList<Producto>> ObtenerTodosAsync(IReadOnlyList<int>? vendedorIdsPermitidos, CancellationToken cancellationToken);

    Task<Producto> CrearAsync(NuevoProducto nuevo, CancellationToken cancellationToken);

    Task ActualizarAsync(int id, IReadOnlyList<int>? vendedorIdsPermitidos, EdicionProducto edicion, CancellationToken cancellationToken);

    Task CambiarActivoAsync(int id, IReadOnlyList<int>? vendedorIdsPermitidos, bool activo, CancellationToken cancellationToken);

    // Gestion de presentaciones (tamaños/precios) de un producto ya existente.
    Task<Presentacion> AgregarPresentacionAsync(int productoId, IReadOnlyList<int>? vendedorIdsPermitidos, PresentacionInput input, CancellationToken cancellationToken);

    Task EditarPresentacionAsync(int presentacionId, IReadOnlyList<int>? vendedorIdsPermitidos, PresentacionInput input, CancellationToken cancellationToken);

    Task CambiarActivaPresentacionAsync(int presentacionId, IReadOnlyList<int>? vendedorIdsPermitidos, bool activo, CancellationToken cancellationToken);
}
