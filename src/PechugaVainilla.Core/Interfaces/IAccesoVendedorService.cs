namespace PechugaVainilla.Core.Interfaces;

// Resuelve "a que Vendedor pertenece este usuario" - lo usan los controllers del panel
// para decidir que puede ver/editar alguien con rol Vendedor (un Administrador no lo necesita,
// el ve todo sin filtro).
public interface IAccesoVendedorService
{
    Task<int?> ObtenerVendedorIdAsync(string usuarioId, CancellationToken cancellationToken);
}
