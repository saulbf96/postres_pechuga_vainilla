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
    Task<IReadOnlyList<Producto>> ObtenerTodosAsync(int? vendedorId, CancellationToken cancellationToken);

    Task<Producto> CrearAsync(NuevoProducto nuevo, CancellationToken cancellationToken);

    Task ActualizarAsync(int id, int? vendedorIdPermitido, EdicionProducto edicion, CancellationToken cancellationToken);

    Task CambiarActivoAsync(int id, int? vendedorIdPermitido, bool activo, CancellationToken cancellationToken);

    // Gestion de presentaciones (tamaños/precios) de un producto ya existente.
    Task<Presentacion> AgregarPresentacionAsync(int productoId, int? vendedorIdPermitido, PresentacionInput input, CancellationToken cancellationToken);

    Task EditarPresentacionAsync(int presentacionId, int? vendedorIdPermitido, PresentacionInput input, CancellationToken cancellationToken);

    Task CambiarActivaPresentacionAsync(int presentacionId, int? vendedorIdPermitido, bool activo, CancellationToken cancellationToken);
}
