using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Core.Interfaces;

public record ItemCarrito(int ProductoId, int PresentacionId, int Cantidad, string? Notas);

// Un checkout es un solo pedido (ya no se separa por vendedor): una entrega para todo el carrito.
public record SolicitudCheckout(
    string NombreCliente,
    string WhatsApp,
    int PuntoEntregaId,
    DateOnly FechaEntrega,
    TimeOnly HoraEntrega,
    string? DetalleEntrega,
    MetodoPago MetodoPago,
    IReadOnlyList<ItemCarrito> Items);

// Excepcion para reglas de negocio violadas (carrito vacio, fuera de horario, etc.) - el
// controller la atrapa y regresa 400 con el mensaje, nunca un 500 con detalles internos.
public class ReglaDeNegocioException(string mensaje) : Exception(mensaje);

public interface IPedidoService
{
    Task<Pedido> CrearPedidoAsync(SolicitudCheckout solicitud, string usuarioId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Pedido>> ObtenerMisPedidosAsync(string usuarioId, CancellationToken cancellationToken);

    Task<Pedido?> ObtenerPedidoAsync(int id, string usuarioId, CancellationToken cancellationToken);

    // Panel: cualquier Administrador ve todos los pedidos, sin filtro.
    Task<IReadOnlyList<Pedido>> ObtenerTodosAsync(CancellationToken cancellationToken);

    Task CambiarEstadoAsync(int id, EstadoPedido nuevoEstado, CancellationToken cancellationToken);

    Task CambiarEstadoPagoAsync(int id, EstadoPago nuevoEstado, CancellationToken cancellationToken);

    // Cuenta pedidos "Nuevo" - para el avisito en el panel.
    Task<int> ContarNuevosAsync(CancellationToken cancellationToken);
}
