namespace PechugaVainilla.Core.Entities;

public class PedidoDetalle
{
    public int Id { get; set; }

    public int PedidoId { get; set; }
    public required Pedido Pedido { get; set; }

    public int ProductoId { get; set; }
    public int PresentacionId { get; set; }

    // Nombre y precio se copian al momento de la compra: si el producto cambia de nombre
    // o de precio despues, este pedido ya hecho no se altera (regla de CLAUDE.md).
    public required string NombreProducto { get; set; }
    public decimal PrecioUnitario { get; set; }

    public int Cantidad { get; set; }
    public string? Notas { get; set; }
}
