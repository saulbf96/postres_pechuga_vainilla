using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Api.Dtos;

public record AdminPuntoEntregaDto(
    int Id,
    int VendedorId,
    string VendedorNombre,
    string Nombre,
    string Tipo,
    List<string> DiasSemana,
    string HoraInicio,
    string HoraFin,
    int DiasAnticipacion,
    string HoraLimitePedido,
    decimal CostoEnvio,
    bool Activo);

public record GuardarPuntoEntregaRequest(
    int? VendedorId,
    string Nombre,
    TipoPuntoEntrega Tipo,
    List<string> DiasSemana,
    string HoraInicio,
    string HoraFin,
    int DiasAnticipacion,
    string HoraLimitePedido,
    decimal CostoEnvio);
