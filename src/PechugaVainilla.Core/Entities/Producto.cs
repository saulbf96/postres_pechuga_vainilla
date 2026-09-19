namespace PechugaVainilla.Core.Entities;

public class Producto
{
    public int Id { get; set; }

    // Clave foranea: a que vendedor pertenece este producto
    public int VendedorId { get; set; }
    public required Vendedor Vendedor { get; set; }

    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Alergenos { get; set; }
    public string? FotoRuta { get; set; }

    // Limite de unidades por dia (perecedero). Null = sin limite
    public int? MaxPorDia { get; set; }

    public bool Activo { get; set; } = true;

     // Navegacion inversa: todas las presentaciones de este producto
    public ICollection<Presentacion> Presentaciones { get; set; } = new List<Presentacion>();
}