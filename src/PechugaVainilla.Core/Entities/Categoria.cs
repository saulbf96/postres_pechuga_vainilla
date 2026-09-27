namespace PechugaVainilla.Core.Entities;

// Vainilla y Pechuga: ya no tienen dueño ni cuenta propia, solo agrupan productos
// y le dan su color/mascota al catalogo (ver docs/PROYECTO.md V3).
public class Categoria
{
    public int Id { get; set; }
    public required string Nombre { get; set; }

    // Version de nombre para usar en Urls sin espacios ni acentos
    public required string Slug { get; set; }

    public string? Descripcion { get; set; }

    // Colores de marca para pintar tarjetas, chips y botones de esta categoria
    public required string Color { get; set; }
    public required string ColorSuave { get; set; }

    public string? FotoRuta { get; set; }

    // Orden de aparicion en el catalogo y en los filtros
    public int Orden { get; set; }

    public bool Activa { get; set; } = true;

    // Navegacion inversa: todos los productos de esta categoria
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();

    // Navegacion inversa: en que puntos de entrega se entrega esta categoria
    public ICollection<PuntoEntregaCategoria> PuntosEntrega { get; set; } = new List<PuntoEntregaCategoria>();
}
