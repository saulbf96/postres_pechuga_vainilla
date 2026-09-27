using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Presentacion> Presentaciones => Set<Presentacion>();
    public DbSet<PuntoEntrega> PuntosEntrega => Set<PuntoEntrega>();
    public DbSet<PuntoEntregaCategoria> PuntosEntregaCategorias => Set<PuntoEntregaCategoria>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity necesita configurar sus tablas primero

        // Renombra las tablas de Identity (por default salen como AspNetUsers, AspNetRoles...)
        // a nombres en español, consistentes con el resto de la base de datos.
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");

            // PasswordCifrada solo la escribe CifradoContrasena con SQL directo. Sin esto, cada
            // UserManager.UpdateAsync (asignar rol, login, etc.) la sobrescribia con NULL.
            entity.Property(u => u.PasswordCifrada).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(u => u.PasswordCifrada).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        });
        modelBuilder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
        modelBuilder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UsuarioRoles"));
        modelBuilder.Entity<IdentityUserClaim<string>>(entity => entity.ToTable("UsuarioClaims"));
        modelBuilder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UsuarioLogins"));
        modelBuilder.Entity<IdentityUserToken<string>>(entity => entity.ToTable("UsuarioTokens"));
        modelBuilder.Entity<IdentityRoleClaim<string>>(entity => entity.ToTable("RolClaims"));

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            entity.HasIndex(c => c.Slug).IsUnique(); // no puede haber dos categorias con el mismo slug
            entity.Property(c => c.Descripcion).HasMaxLength(500);
            entity.Property(c => c.Color).HasMaxLength(20).IsRequired();
            entity.Property(c => c.ColorSuave).HasMaxLength(20).IsRequired();
            entity.Property(c => c.FotoRuta).HasMaxLength(300);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Descripcion).HasMaxLength(500);
            entity.Property(p => p.Alergenos).HasMaxLength(300);
            entity.Property(p => p.FotoRuta).HasMaxLength(300);

            entity.HasOne(p => p.Categoria)
                  .WithMany(c => c.Productos)
                  .HasForeignKey(p => p.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict); // no permite borrar una categoria si tiene productos
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
        });

        // Tabla intermedia con clave compuesta: que categorias se entregan en cada punto.
        modelBuilder.Entity<PuntoEntregaCategoria>(entity =>
        {
            entity.HasKey(pec => new { pec.PuntoEntregaId, pec.CategoriaId });

            entity.HasOne(pec => pec.PuntoEntrega)
                  .WithMany(pe => pe.Categorias)
                  .HasForeignKey(pec => pec.PuntoEntregaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pec => pec.Categoria)
                  .WithMany(c => c.PuntosEntrega)
                  .HasForeignKey(pec => pec.CategoriaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.Property(p => p.NombreCliente).HasMaxLength(150).IsRequired();
            entity.Property(p => p.WhatsApp).HasMaxLength(20).IsRequired();
            entity.Property(p => p.DetalleEntrega).HasMaxLength(300);
            entity.Property(p => p.Total).HasPrecision(10, 2);
            entity.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);

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
    }
}
