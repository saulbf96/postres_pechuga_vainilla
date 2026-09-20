using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Api.Dtos;

public record AdminPedidoDto(
    int Id,
    string CheckoutId,
    string VendedorNombre,
    string NombreCliente,
    string WhatsAppCliente,
    string PuntoEntregaNombre,
    DateOnly FechaEntrega,
    TimeOnly HoraEntrega,
    string? DetalleEntrega,
    decimal Total,
    string Estado,
    string EstadoPago,
    List<PedidoDetalleDto> Detalles);

public record CambiarEstadoPedidoRequest(EstadoPedido Estado);

public record CambiarEstadoPagoRequest(EstadoPago Estado);
