namespace PechugaVainilla.Api.Dtos;

public record PerfilDto(string Id, string Nombre, string Email, string? WhatsApp, List<string> Roles, bool DebeCambiarPassword);

// Sin contraseña: el sistema genera una temporal.
public record CrearUsuarioRequest(string Nombre, string Email, string? WhatsApp, string Rol);

// Correo y contraseña de un administrador para autorizar expirar/restablecer contraseñas.
public record AutorizacionAdminRequest(string EmailAdmin, string PasswordAdmin);
