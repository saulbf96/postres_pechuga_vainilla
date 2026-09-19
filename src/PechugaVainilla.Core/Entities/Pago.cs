using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Core.Entities;

public class Pago
{
    public int Id { get; set; }

    public int PedidoId { get; set; }
    public required Pedido Pedido { get; set; }

    public MetodoPago Metodo { get; set; }
    public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;
    public decimal Monto { get; set; }

    // Id de la transaccion en la pasarela (se llena en la Etapa 4, con el webhook)
    public string? ReferenciaProveedor { get; set; }
    public DateTime? PagadoEn { get; set; }

    // Que vendedor recibio el efectivo/transferencia, para los cortes (Etapa 3)
    public int? CobradoPorVendedorId { get; set; }
}
