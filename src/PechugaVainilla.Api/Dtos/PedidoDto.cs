using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Api.Dtos;

public record ItemCarritoRequest(int ProductoId, int PresentacionId, int Cantidad, string? Notas);

// Un checkout es un solo pedido (ya no se separa por vendedor): una entrega para todo el carrito.
public record CheckoutRequest(
    string NombreCliente,
    string WhatsApp,
    int PuntoEntregaId,
    DateOnly FechaEntrega,
    TimeOnly HoraEntrega,
    string? DetalleEntrega,
    MetodoPago MetodoPago,
    List<ItemCarritoRequest> Items);

public record PedidoDetalleDto(int Id, string NombreProducto, int CategoriaId, decimal PrecioUnitario, int Cantidad, string? Notas);

public record PedidoDto(
    int Id,
    string PuntoEntregaNombre,
    DateOnly FechaEntrega,
    TimeOnly HoraEntrega,
    string? DetalleEntrega,
    decimal Total,
    string Estado,
    string EstadoPago,
    List<PedidoDetalleDto> Detalles);
