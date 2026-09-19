using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Entities;

namespace PechugaVainilla.Infrastructure.Data;

public class PechugaVainillaDbContext : DbContext
{
    public PechugaVainillaDbContext(DbContextOptions<PechugaVainillaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vendedor> Vendedores => Set<Vendedor>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Presentacion> Presentaciones => Set<Presentacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vendedor>(entity =>
        {
            entity.Property(v => v.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(v => v.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(v => v.Slug).IsUnique(); // no puede haber dos vendedores con el mismo slug
            entity.Property(v => v.WhatsApp).HasMaxLength(20).IsRequired();
            entity.Property(v => v.Descripcion).HasMaxLength(500);
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
    }
}