using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Core.Entities;

public class PuntoEntrega
{
    public int Id { get; set; }

    public int VendedorId { get; set; }
    public required Vendedor Vendedor { get; set; }

    public required string Nombre { get; set; }
    public TipoPuntoEntrega Tipo { get; set; }
    public DiasSemana DiasSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    // Con cuantos dias de anticipacion minima se puede pedir (ej. 1 = no se puede pedir para hoy)
    public int DiasAnticipacion { get; set; }

    // Hora limite del dia anterior (o del dia permitido) para aceptar pedidos
    public TimeOnly HoraLimitePedido { get; set; }

    public decimal CostoEnvio { get; set; }
    public bool Activo { get; set; } = true;
}
