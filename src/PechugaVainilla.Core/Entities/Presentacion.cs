namespace PechugaVainilla.Core.Entities;

public class Presentacion
{
    public int Id { get; set; }

    // Clave foranea: a que producto pertenece esta presentacion
    public int ProductoId { get; set; }
    public required Producto Producto { get; set; }

    // Ej: "Chico", "Mediano", "Grande", "Pieza"
    public required string Nombre { get; set; }

    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;
}