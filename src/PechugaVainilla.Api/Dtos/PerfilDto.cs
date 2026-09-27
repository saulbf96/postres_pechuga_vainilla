namespace PechugaVainilla.Api.Dtos;

public record PerfilDto(string Id, string Nombre, string Email, string? WhatsApp, List<string> Roles);

public record CrearUsuarioRequest(string Nombre, string Email, string? WhatsApp, string Password, string Rol);
