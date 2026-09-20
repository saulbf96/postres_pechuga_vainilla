namespace PechugaVainilla.Api.Dtos;

public record AsignacionDto(int Id, int VendedorId, string VendedorNombre, List<string> DiasSemana, string HoraInicio, string HoraFin, bool Activo);

public record PerfilDto(string Id, string Nombre, string Email, string? WhatsApp, List<string> Roles, List<AsignacionDto> Asignaciones);

public record NuevaAsignacionRequest(int VendedorId, List<string> DiasSemana, string HoraInicio, string HoraFin);

public record CrearUsuarioRequest(string Nombre, string Email, string? WhatsApp, string Password, string Rol, List<NuevaAsignacionRequest> Asignaciones);
