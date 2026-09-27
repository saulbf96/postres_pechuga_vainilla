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

    public async Task<IReadOnlyList<Producto>> ObtenerActivosAsync(int? categoriaId, CancellationToken cancellationToken)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Presentaciones)
            .Where(p => p.Activo);

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        return await query.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken)
    {
        return await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Presentaciones)
            .OrderBy(p => p.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Producto> CrearAsync(NuevoProducto nuevo, CancellationToken cancellationToken)
    {
        var categoria = await _context.Categorias.FirstOrDefaultAsync(c => c.Id == nuevo.CategoriaId, cancellationToken)
            ?? throw new ReglaDeNegocioException("La categoria no existe.");

        var producto = new Producto
        {
            Categoria = categoria,
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

    public async Task ActualizarAsync(int id, EdicionProducto edicion, CancellationToken cancellationToken)
    {
        var producto = await ObtenerOFallarAsync(id, cancellationToken);

        producto.Nombre = edicion.Nombre;
        producto.Descripcion = edicion.Descripcion;
        producto.Alergenos = edicion.Alergenos;
        producto.MaxPorDia = edicion.MaxPorDia;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarActivoAsync(int id, bool activo, CancellationToken cancellationToken)
    {
        var producto = await ObtenerOFallarAsync(id, cancellationToken);
        producto.Activo = activo;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Presentacion> AgregarPresentacionAsync(int productoId, PresentacionInput input, CancellationToken cancellationToken)
    {
        var producto = await ObtenerOFallarAsync(productoId, cancellationToken);

        var presentacion = new Presentacion { Producto = producto, Nombre = input.Nombre, Precio = input.Precio };
        producto.Presentaciones.Add(presentacion);

        await _context.SaveChangesAsync(cancellationToken);
        return presentacion;
    }

    public async Task EditarPresentacionAsync(int presentacionId, PresentacionInput input, CancellationToken cancellationToken)
    {
        var presentacion = await ObtenerPresentacionOFallarAsync(presentacionId, cancellationToken);
        presentacion.Nombre = input.Nombre;
        presentacion.Precio = input.Precio;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CambiarActivaPresentacionAsync(int presentacionId, bool activo, CancellationToken cancellationToken)
    {
        var presentacion = await ObtenerPresentacionOFallarAsync(presentacionId, cancellationToken);
        presentacion.Activo = activo;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Producto> ObtenerOFallarAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Productos
            .Include(p => p.Presentaciones)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new ReglaDeNegocioException("El producto no existe.");
    }

    private async Task<Presentacion> ObtenerPresentacionOFallarAsync(int presentacionId, CancellationToken cancellationToken)
    {
        return await _context.Presentaciones
            .Include(pr => pr.Producto)
            .FirstOrDefaultAsync(pr => pr.Id == presentacionId, cancellationToken)
            ?? throw new ReglaDeNegocioException("La presentacion no existe.");
    }
}
