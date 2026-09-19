namespace PechugaVainilla.Api.Dtos;

public record RegistroRequest(string Nombre, string Email, string WhatsApp, string Password);

public record LoginRequest(string Email, string Password);

public record UsuarioDto(string Id, string Nombre, string Email, string? WhatsApp, IList<string> Roles);
