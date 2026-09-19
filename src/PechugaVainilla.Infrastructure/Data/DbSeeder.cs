using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(PechugaVainillaDbContext context)
    {
        var vainilla = context.Vendedores.FirstOrDefault(v => v.Slug == "vainilla");
        var pechuga = context.Vendedores.FirstOrDefault(v => v.Slug == "pechuga");

        // Cada bloque revisa su propia condicion, para poder agregar cosas nuevas (como los
        // puntos de entrega) sin que un "ya hay datos" general se salte todo lo demas.
        if (vainilla is null && pechuga is null)
        {
            vainilla = new Vendedor
            {
                Nombre = "Vainilla",
                Slug = "vainilla",
                WhatsApp = "5215500000001",
                Descripcion = "Postres caseros: fresas con crema, panquesitos y mas."
            };

            pechuga = new Vendedor
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

        if (!context.PuntosEntrega.Any() && vainilla is not null && pechuga is not null)
        {
            const DiasSemana todaLaSemana = DiasSemana.Lunes | DiasSemana.Martes | DiasSemana.Miercoles
                | DiasSemana.Jueves | DiasSemana.Viernes | DiasSemana.Sabado | DiasSemana.Domingo;
            const DiasSemana entreSemana = DiasSemana.Lunes | DiasSemana.Martes | DiasSemana.Miercoles
                | DiasSemana.Jueves | DiasSemana.Viernes;

            context.PuntosEntrega.AddRange(
                new PuntoEntrega
                {
                    Vendedor = vainilla,
                    Nombre = "Recoger en casa",
                    Tipo = TipoPuntoEntrega.Recoger,
                    DiasSemana = todaLaSemana,
                    HoraInicio = new TimeOnly(10, 0),
                    HoraFin = new TimeOnly(20, 0),
                    DiasAnticipacion = 1,
                    HoraLimitePedido = new TimeOnly(20, 0),
                    CostoEnvio = 0m,
                },
                new PuntoEntrega
                {
                    Vendedor = vainilla,
                    Nombre = "A domicilio en la zona",
                    Tipo = TipoPuntoEntrega.Domicilio,
                    DiasSemana = DiasSemana.Miercoles | DiasSemana.Jueves | DiasSemana.Viernes | DiasSemana.Sabado,
                    HoraInicio = new TimeOnly(12, 0),
                    HoraFin = new TimeOnly(19, 0),
                    DiasAnticipacion = 1,
                    HoraLimitePedido = new TimeOnly(20, 0),
                    CostoEnvio = 35.00m,
                },
                new PuntoEntrega
                {
                    Vendedor = pechuga,
                    Nombre = "Punto UAM",
                    Tipo = TipoPuntoEntrega.Campus,
                    DiasSemana = entreSemana,
                    HoraInicio = new TimeOnly(11, 0),
                    HoraFin = new TimeOnly(15, 0),
                    DiasAnticipacion = 1,
                    HoraLimitePedido = new TimeOnly(21, 0),
                    CostoEnvio = 0m,
                },
                new PuntoEntrega
                {
                    Vendedor = pechuga,
                    Nombre = "Punto UNAM (sabados)",
                    Tipo = TipoPuntoEntrega.Campus,
                    DiasSemana = DiasSemana.Sabado,
                    HoraInicio = new TimeOnly(10, 0),
                    HoraFin = new TimeOnly(14, 0),
                    DiasAnticipacion = 2,
                    HoraLimitePedido = new TimeOnly(21, 0),
                    CostoEnvio = 0m,
                });

            context.SaveChanges();
        }
    }
}
