using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Api.Dtos;

public record ItemCarritoRequest(int ProductoId, int PresentacionId, int Cantidad, string? Notas);

public record GrupoCheckoutRequest(int VendedorId, int PuntoEntregaId, DateOnly FechaEntrega, TimeOnly HoraEntrega, List<ItemCarritoRequest> Items);

public record CheckoutRequest(string NombreCliente, string WhatsApp, string? DetalleEntrega, MetodoPago MetodoPago, List<GrupoCheckoutRequest> Grupos);

public record PedidoDetalleDto(int Id, string NombreProducto, decimal PrecioUnitario, int Cantidad, string? Notas);

public record PedidoDto(
    int Id,
    string CheckoutId,
    string VendedorNombre,
    string PuntoEntregaNombre,
    DateOnly FechaEntrega,
    TimeOnly HoraEntrega,
    string? DetalleEntrega,
    decimal Total,
    string Estado,
    string EstadoPago,
    List<PedidoDetalleDto> Detalles);
