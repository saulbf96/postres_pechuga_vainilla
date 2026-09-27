namespace PechugaVainilla.Core.Interfaces;

public record PerfilUsuario(
    string Id,
    string Nombre,
    string Email,
    string? WhatsApp,
    IReadOnlyList<string> Roles,
    bool DebeCambiarPassword);

// Ya no trae contraseña: la genera el sistema (contraseña temporal).
public record NuevoUsuarioPanel(string Nombre, string Email, string? WhatsApp, string Rol);

public record UsuarioCreado(string Id, string PasswordTemporal);

// Solo para Administrador: dar de alta usuarios y administrar sus contraseñas.
public interface IPerfilesService
{
    Task<IReadOnlyList<PerfilUsuario>> ObtenerTodosAsync(CancellationToken cancellationToken);

    // Lanza ReglaDeNegocioException si el correo ya existe.
    Task<UsuarioCreado> CrearUsuarioAsync(NuevoUsuarioPanel nuevo, CancellationToken cancellationToken);

    // true si el correo y contraseña son de una cuenta con rol Administrador.
    Task<bool> VerificarAdministradorAsync(string email, string password);

    // La contraseña actual sigue sirviendo para entrar, pero al entrar se le pide una nueva.
    Task ExpirarPasswordAsync(string usuarioId, CancellationToken cancellationToken);

    // Reemplaza la contraseña por una temporal (para quien la olvido) y la regresa.
    Task<string> RestablecerPasswordAsync(string usuarioId, CancellationToken cancellationToken);
}
