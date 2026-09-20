using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;

namespace PechugaVainilla.Core.Interfaces;

public record PerfilUsuario(
    string Id,
    string Nombre,
    string Email,
    string? WhatsApp,
    IReadOnlyList<string> Roles,
    IReadOnlyList<AsignacionVendedor> Asignaciones);

public record NuevaAsignacion(int VendedorId, DiasSemana DiasSemana, TimeOnly HoraInicio, TimeOnly HoraFin);

public record NuevoUsuarioPanel(string Nombre, string Email, string? WhatsApp, string Password, string Rol, IReadOnlyList<NuevaAsignacion> Asignaciones);

// Solo para Administrador: dar de alta gente (tu hermana, un repartidor...), su rol,
// y a que vendedor(es) puede ayudar a entregar y en que dias/horario.
public interface IPerfilesService
{
    Task<IReadOnlyList<PerfilUsuario>> ObtenerTodosAsync(CancellationToken cancellationToken);

    // Lanza ReglaDeNegocioException si el correo ya existe o la contraseña no cumple la politica.
    Task<string> CrearUsuarioAsync(NuevoUsuarioPanel nuevo, CancellationToken cancellationToken);

    Task<AsignacionVendedor> AgregarAsignacionAsync(string usuarioId, NuevaAsignacion asignacion, CancellationToken cancellationToken);

    Task CambiarActivaAsignacionAsync(int asignacionId, bool activo, CancellationToken cancellationToken);
}
