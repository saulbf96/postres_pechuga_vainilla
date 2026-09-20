using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Core.Entities;

// Le da acceso a un usuario (normalmente un empleado, no el dueño) para ver y manejar
// los pedidos/productos de UN vendedor especifico, en ciertos dias/horario. Un usuario
// puede tener varias asignaciones (ej. entrega Pechuga entre semana y Vainilla el sabado).
public class AsignacionVendedor
{
    public int Id { get; set; }

    public required string UsuarioId { get; set; }

    public int VendedorId { get; set; }
    public required Vendedor Vendedor { get; set; }

    public DiasSemana DiasSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }

    public bool Activo { get; set; } = true;
}
