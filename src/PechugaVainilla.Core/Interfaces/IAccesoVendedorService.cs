namespace PechugaVainilla.Core.Interfaces;

// Resuelve a que Vendedor(es) tiene acceso un usuario - lo usan los controllers del panel
// para decidir que puede ver/editar alguien con rol Vendedor. Un usuario puede tener acceso
// a mas de uno (es dueño de uno y ademas ayuda a entregar de otro, por ejemplo).
// Un Administrador no lo necesita, el ve todo sin filtro (lista null en los controllers).
public interface IAccesoVendedorService
{
    Task<IReadOnlyList<int>> ObtenerVendedorIdsAsync(string usuarioId, CancellationToken cancellationToken);
}
