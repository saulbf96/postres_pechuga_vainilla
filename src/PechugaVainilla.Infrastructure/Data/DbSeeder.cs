using PechugaVainilla.Core.Entities;

namespace PechugaVainilla.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(PechugaVainillaDbContext context)
    {
        if (context.Vendedores.Any())
        {
            return; // ya hay datos, no insertar de nuevo
        }

        var vainilla = new Vendedor
        {
            Nombre = "Vainilla",
            Slug = "vainilla",
            WhatsApp = "5215500000001",
            Descripcion = "Postres caseros: fresas con crema, panquesitos y mas."
        };

        var pechuga = new Vendedor
        {
            Nombre = "Pechuga",
            Slug = "pechuga",
            WhatsApp = "5215500000002",
            Descripcion = "Antojos salados: burritos, cuernitos, chapatas y mas."
        };

        context.Vendedores.AddRange(vainilla, pechuga);

        var fresas = new Producto { Vendedor = vainilla, Nombre = "Fresas con crema", Descripcion = "Fresas frescas con crema batida", MaxPorDia = 20 };
        fresas.Presentaciones.Add(new Presentacion { Producto = fresas, Nombre = "Chico", Precio = 45.00m });
        fresas.Presentaciones.Add(new Presentacion { Producto = fresas, Nombre = "Grande", Precio = 75.00m });

        var panquesitos = new Producto { Vendedor = vainilla, Nombre = "Panquesitos", Descripcion = "Panquesitos de vainilla con betun", MaxPorDia = 30 };
        panquesitos.Presentaciones.Add(new Presentacion { Producto = panquesitos, Nombre = "Pieza", Precio = 25.00m });

        var burrito = new Producto { Vendedor = pechuga, Nombre = "Burrito", Descripcion = "Burrito de frijol con queso", MaxPorDia = 15 };
        burrito.Presentaciones.Add(new Presentacion { Producto = burrito, Nombre = "Chico", Precio = 35.00m });
        burrito.Presentaciones.Add(new Presentacion { Producto = burrito, Nombre = "Grande", Precio = 55.00m });

        var cuernito = new Producto { Vendedor = pechuga, Nombre = "Cuernito", Descripcion = "Cuernito de jamon y queso", MaxPorDia = 20 };
        cuernito.Presentaciones.Add(new Presentacion { Producto = cuernito, Nombre = "Pieza", Precio = 28.00m });

        var chapata = new Producto { Vendedor = pechuga, Nombre = "Chapata", Descripcion = "Chapata de pierna con aguacate", MaxPorDia = 15 };
        chapata.Presentaciones.Add(new Presentacion { Producto = chapata, Nombre = "Pieza", Precio = 48.00m });

        context.Productos.AddRange(fresas, panquesitos, burrito, cuernito, chapata);

        context.SaveChanges();
    }
}
