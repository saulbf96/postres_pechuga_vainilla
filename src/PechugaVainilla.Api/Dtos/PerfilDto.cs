namespace PechugaVainilla.Api.Dtos;

public record PerfilDto(string Id, string Nombre, string Email, string? WhatsApp, List<string> Roles, bool DebeCambiarPassword, bool Activo);

// Sin contraseña: el sistema genera una temporal.
public record CrearUsuarioRequest(string Nombre, string Email, string? WhatsApp, string Rol);

// Datos del administrador solo son obligatorios si la edicion cambia el rol.
public record EditarUsuarioRequest(string Nombre, string Email, string? WhatsApp, string Rol, string? EmailAdmin, string? PasswordAdmin);

// Correo y contraseña de un administrador para autorizar acciones delicadas
// (restablecer contraseña, eliminar/reactivar, cambiar rol).
public record AutorizacionAdminRequest(string EmailAdmin, string PasswordAdmin);
