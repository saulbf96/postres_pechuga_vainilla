using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Core.Entities;

public class Pedido
{
    public int Id { get; set; }

    // Un checkout con productos de los dos vendedores se divide en un Pedido por vendedor,
    // todos comparten el mismo CheckoutId para poder mostrarlos juntos en la confirmacion.
    public required string CheckoutId { get; set; }

    // Id del usuario de Identity (string). Requerido en este flujo (el carrito exige login);
    // se deja nullable pensando en un futuro checkout de invitado.
    public string? UsuarioId { get; set; }

    public required string NombreCliente { get; set; }
    public required string WhatsApp { get; set; }

    public int VendedorId { get; set; }
    public required Vendedor Vendedor { get; set; }

    public int PuntoEntregaId { get; set; }
    public required PuntoEntrega PuntoEntrega { get; set; }

    public DateOnly FechaEntrega { get; set; }
    public TimeOnly HoraEntrega { get; set; }
    public string? DetalleEntrega { get; set; }

    // Se recalcula siempre en el servidor (suma de PedidoDetalle.PrecioUnitario * Cantidad + costo de envio).
    public decimal Total { get; set; }

    public EstadoPedido Estado { get; set; } = EstadoPedido.Nuevo;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    public Pago? Pago { get; set; }
}
