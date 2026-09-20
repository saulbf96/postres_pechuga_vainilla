using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Infrastructure.Data;

// IdentityDbContext ya trae las tablas de Identity (usuarios, roles, claims, logins...)
// ademas de las nuestras.
public class PechugaVainillaDbContext : IdentityDbContext<Usuario>
{
    public PechugaVainillaDbContext(DbContextOptions<PechugaVainillaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vendedor> Vendedores => Set<Vendedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Presentacion> Presentaciones => Set<Presentacion>();
    public DbSet<PuntoEntrega> PuntosEntrega => Set<PuntoEntrega>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<AsignacionVendedor> AsignacionesVendedor => Set<AsignacionVendedor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity necesita configurar sus tablas primero

        modelBuilder.Entity<Vendedor>(entity =>
        {
            entity.Property(v => v.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(v => v.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(v => v.Slug).IsUnique(); // no puede haber dos vendedores con el mismo slug
            entity.Property(v => v.WhatsApp).HasMaxLength(20).IsRequired();
            entity.Property(v => v.Descripcion).HasMaxLength(500);

            // Si se borra la cuenta de Identity, el Vendedor se queda (solo pierde el dueño).
            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(v => v.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Descripcion).HasMaxLength(500);
            entity.Property(p => p.Alergenos).HasMaxLength(300);
            entity.Property(p => p.FotoRuta).HasMaxLength(300);

            entity.HasOne(p => p.Vendedor)
                  .WithMany(v => v.Productos)
                  .HasForeignKey(p => p.VendedorId)
                  .OnDelete(DeleteBehavior.Restrict); // no permite borrar un vendedor si tiene productos
        });

        modelBuilder.Entity<Presentacion>(entity =>
        {
            entity.Property(pr => pr.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(pr => pr.Precio).HasPrecision(10, 2); // decimal(10,2)
            entity.HasOne(pr => pr.Producto)
                  .WithMany(p => p.Presentaciones)
                  .HasForeignKey(pr => pr.ProductoId)
                  .OnDelete(DeleteBehavior.Cascade); // si se borra el producto, se borran sus presentaciones
        });

        modelBuilder.Entity<PuntoEntrega>(entity =>
        {
            entity.Property(pe => pe.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(pe => pe.Tipo).HasConversion<string>().HasMaxLength(20);
            entity.Property(pe => pe.DiasSemana).HasConversion<string>().HasMaxLength(100);
            entity.Property(pe => pe.CostoEnvio).HasPrecision(10, 2);

            entity.HasOne(pe => pe.Vendedor)
                  .WithMany(v => v.PuntosEntrega)
                  .HasForeignKey(pe => pe.VendedorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.Property(p => p.CheckoutId).HasMaxLength(64).IsRequired();
            entity.HasIndex(p => p.CheckoutId);
            entity.Property(p => p.NombreCliente).HasMaxLength(150).IsRequired();
            entity.Property(p => p.WhatsApp).HasMaxLength(20).IsRequired();
            entity.Property(p => p.DetalleEntrega).HasMaxLength(300);
            entity.Property(p => p.Total).HasPrecision(10, 2);
            entity.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(p => p.Vendedor)
                  .WithMany()
                  .HasForeignKey(p => p.VendedorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.PuntoEntrega)
                  .WithMany()
                  .HasForeignKey(p => p.PuntoEntregaId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Un usuario puede tener muchos pedidos; si se borra el usuario, no se borran sus pedidos
            // (quedan como historial), por eso SetNull en vez de Cascade.
            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(p => p.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PedidoDetalle>(entity =>
        {
            entity.Property(pd => pd.NombreProducto).HasMaxLength(100).IsRequired();
            entity.Property(pd => pd.PrecioUnitario).HasPrecision(10, 2);
            entity.Property(pd => pd.Notas).HasMaxLength(200);

            entity.HasOne(pd => pd.Pedido)
                  .WithMany(p => p.Detalles)
                  .HasForeignKey(pd => pd.PedidoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.Property(pa => pa.Metodo).HasConversion<string>().HasMaxLength(20);
            entity.Property(pa => pa.Estado).HasConversion<string>().HasMaxLength(20);
            entity.Property(pa => pa.Monto).HasPrecision(10, 2);
            entity.Property(pa => pa.ReferenciaProveedor).HasMaxLength(100);

            entity.HasOne(pa => pa.Pedido)
                  .WithOne(p => p.Pago)
                  .HasForeignKey<Pago>(pa => pa.PedidoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AsignacionVendedor>(entity =>
        {
            entity.Property(a => a.DiasSemana).HasConversion<string>().HasMaxLength(100);

            entity.HasOne(a => a.Vendedor)
                  .WithMany()
                  .HasForeignKey(a => a.VendedorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
