
namespace PechugaVainilla.Core.Entities;

public class Vendedor
{
    public int Id { get; set; }
    public required string Nombre { get; set; }

    //version de nombre para usar Urls sin espacion ni acento
    public required string Slug { get; set; }
    public required string WhatsApp { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    // Id del usuario de Identity dueño de este negocio (rol Vendedor). Nullable porque los
    // vendedores sembrados de ejemplo no tienen cuenta hasta que se vincula una (Etapa 3).
    public string? UsuarioId { get; set; }

    //navegacion inversa  todos los productos  de este vendedor

    public ICollection<Producto> Productos {get;set;} = new List<Producto>();

    // Navegacion inversa: todos los puntos de entrega de este vendedor
    public ICollection<PuntoEntrega> PuntosEntrega { get; set; } = new List<PuntoEntrega>();


    



}