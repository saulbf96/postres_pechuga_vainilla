namespace PechugaVainilla.Core.Interfaces;

public record PerfilUsuario(
    string Id,
    string Nombre,
    string Email,
    string? WhatsApp,
    IReadOnlyList<string> Roles);

public record NuevoUsuarioPanel(string Nombre, string Email, string? WhatsApp, string Password, string Rol);

// Solo para Administrador: dar de alta al otro administrador (los clientes normalmente se registran solos).
public interface IPerfilesService
{
    Task<IReadOnlyList<PerfilUsuario>> ObtenerTodosAsync(CancellationToken cancellationToken);

    // Lanza ReglaDeNegocioException si el correo ya existe o la contraseña no cumple la politica.
    Task<string> CrearUsuarioAsync(NuevoUsuarioPanel nuevo, CancellationToken cancellationToken);
}
