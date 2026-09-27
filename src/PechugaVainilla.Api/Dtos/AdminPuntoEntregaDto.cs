using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Api.Dtos;

public record AdminPuntoEntregaDto(
    int Id,
    string Nombre,
    string Tipo,
    List<string> DiasSemana,
    string HoraInicio,
    string HoraFin,
    int DiasAnticipacion,
    string HoraLimitePedido,
    decimal CostoEnvio,
    bool Activo,
    List<int> CategoriaIds);

public record GuardarPuntoEntregaRequest(
    string Nombre,
    TipoPuntoEntrega Tipo,
    List<string> DiasSemana,
    string HoraInicio,
    string HoraFin,
    int DiasAnticipacion,
    string HoraLimitePedido,
    decimal CostoEnvio,
    List<int> CategoriaIds);
