namespace PechugaVainilla.Core.Entities;

// Tabla intermedia: que categorias se entregan en cada punto de entrega.
public class PuntoEntregaCategoria
{
    public int PuntoEntregaId { get; set; }
    public required PuntoEntrega PuntoEntrega { get; set; }

    public int CategoriaId { get; set; }
    public required Categoria Categoria { get; set; }
}
