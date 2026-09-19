namespace PechugaVainilla.Api.Dtos;

public record PuntoEntregaDto(
    int Id,
    int VendedorId,
    string Nombre,
    string Tipo,
    List<string> DiasSemana,
    string HoraInicio,
    string HoraFin,
    int DiasAnticipacion,
    string HoraLimitePedido,
    decimal CostoEnvio);
