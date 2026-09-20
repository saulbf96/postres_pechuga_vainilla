using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Services;

// Implementacion real de IProductoService, usando EF Core contra SQL Server.
public class ProductoService : IProductoService
{
    private readonly PechugaVainillaDbContext _context;

    public ProductoService(PechugaVainillaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Producto>> ObtenerActivosAsync(int? vendedorId, CancellationToken cancellationToken)
    {
        var query = _context.Productos
            .Include(p => p.Vendedor)
            .Include(p => p.Presentaciones)
            .Where(p => p.Activo);

        if (vendedorId.HasValue)
        {
            query = query.Where(p => p.VendedorId == vendedorId.Value);
        }

        return await query.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> ObtenerTodosAsync(IReadOnlyList<int>? vendedorIdsPermitidos, CancellationToken cancellationToken)
    {
        var query = _context.Productos
            .Include(p => p.Vendedor)
            .Include(p => p.Presentaciones)
            .AsQueryable();

        if (vendedorIdsPermitidos is not null)
        {
            query = query.Where(p => vendedorIdsPermitidos.Contains(p.VendedorId));
        }

        return await query.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<Producto> CrearAsync(NuevoProducto nuevo, CancellationToken cancellationToken)
    {
        var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.Id == nuevo.VendedorId, cancellationToken)
            ?? throw new ReglaDeNegocioException("El vendedor no existe.");

        var producto = new Producto
        {
            Vendedor = vendedor,
            Nombre = nuevo.Nombre,
            Descripcion = nuevo.Descripcion,
            Alergenos = nuevo.Alergenos,
            MaxPorDia = nuevo.MaxPorDia,
        };

        foreach (var presentacion in nuevo.Presentaciones)
        {
            producto.Presentaciones.Add(new Presentacion
            {
                Producto = producto,
                Nombre = presentacion.Nombre,
                Precio = presentacion.Precio,
            });
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync(cancellationToken);

        return producto;
    }

    public async Task ActualizarAsync(int id, IReadOnlyList<int>? vendedorIdsPermitidos, EdicionProducto edicion, CancellationToken cancellationToken)
    {
        var producto = await ObtenerConPermisoAsync(id, vendedorIdsPermitidos, cancellationToken);

        producto.Nombre = edicion.Nombre;
        producto.Descripcion = edicion.Descripcion;
        producto.Alergenos = edicion.Alergenos;
        producto.MaxPorDia = edicion.MaxPorDia;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarActivoAsync(int id, IReadOnlyList<int>? vendedorIdsPermitidos, bool activo, CancellationToken cancellationToken)
    {
        var producto = await ObtenerConPermisoAsync(id, vendedorIdsPermitidos, cancellationToken);
        producto.Activo = activo;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Presentacion> AgregarPresentacionAsync(int productoId, IReadOnlyList<int>? vendedorIdsPermitidos, PresentacionInput input, CancellationToken cancellationToken)
    {
        var producto = await ObtenerConPermisoAsync(productoId, vendedorIdsPermitidos, cancellationToken);

        var presentacion = new Presentacion { Producto = producto, Nombre = input.Nombre, Precio = input.Precio };
        producto.Presentaciones.Add(presentacion);

        await _context.SaveChangesAsync(cancellationToken);
        return presentacion;
    }

    public async Task EditarPresentacionAsync(int presentacionId, IReadOnlyList<int>? vendedorIdsPermitidos, PresentacionInput input, CancellationToken cancellationToken)
    {
        var presentacion = await ObtenerPresentacionConPermisoAsync(presentacionId, vendedorIdsPermitidos, cancellationToken);
        presentacion.Nombre = input.Nombre;
        presentacion.Precio = input.Precio;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarActivaPresentacionAsync(int presentacionId, IReadOnlyList<int>? vendedorIdsPermitidos, bool activo, CancellationToken cancellationToken)
    {
        var presentacion = await ObtenerPresentacionConPermisoAsync(presentacionId, vendedorIdsPermitidos, cancellationToken);
        presentacion.Activo = activo;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Producto> ObtenerConPermisoAsync(int id, IReadOnlyList<int>? vendedorIdsPermitidos, CancellationToken cancellationToken)
    {
        var producto = await _context.Productos
            .Include(p => p.Presentaciones)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new ReglaDeNegocioException("El producto no existe.");

        // Un Vendedor (no Administrador) solo puede tocar productos de sus vendedores asignados.
        if (vendedorIdsPermitidos is not null && !vendedorIdsPermitidos.Contains(producto.VendedorId))
        {
            throw new ReglaDeNegocioException("No tienes permiso sobre este producto.");
        }

        return producto;
    }

    private async Task<Presentacion> ObtenerPresentacionConPermisoAsync(int presentacionId, IReadOnlyList<int>? vendedorIdsPermitidos, CancellationToken cancellationToken)
    {
        var presentacion = await _context.Presentaciones
            .Include(pr => pr.Producto)
            .FirstOrDefaultAsync(pr => pr.Id == presentacionId, cancellationToken)
            ?? throw new ReglaDeNegocioException("La presentacion no existe.");

        if (vendedorIdsPermitidos is not null && !vendedorIdsPermitidos.Contains(presentacion.Producto.VendedorId))
        {
            throw new ReglaDeNegocioException("No tienes permiso sobre esta presentacion.");
        }

        return presentacion;
    }
}
