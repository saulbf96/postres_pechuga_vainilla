namespace PechugaVainilla.Core.Enums;

// Todos se guardan como texto en la base (HasConversion<string>() en el DbContext),
// para que sean legibles directo en SQL Server sin tener que consultar una tabla de catalogo.

public enum TipoPuntoEntrega
{
    Domicilio,
    Recoger,
    Campus,
}

// [Flags] permite combinar dias con OR bit a bit (ej. Lunes | Miercoles | Viernes).
[Flags]
public enum DiasSemana
{
    Ninguno = 0,
    Lunes = 1,
    Martes = 2,
    Miercoles = 4,
    Jueves = 8,
    Viernes = 16,
    Sabado = 32,
    Domingo = 64,
}

public enum EstadoPedido
{
    Nuevo,
    Preparando,
    Listo,
    Entregado,
    Cancelado,
}

public enum EstadoPago
{
    Pendiente,
    Pagado,
    PorConfirmar,
    PorCobrar,
    Cobrado,
}

public enum MetodoPago
{
    Tarjeta,
    SPEI,
    OXXO,
    Efectivo,
    Transferencia,
}
