using Microsoft.AspNetCore.Identity;

namespace PechugaVainilla.Infrastructure.Identity;

// Extiende IdentityUser (que ya trae Email, PhoneNumber, hash de contraseña, etc.)
// Reutilizamos PhoneNumber para el WhatsApp en vez de duplicar el campo.

public class Usuario : IdentityUser
{
    public required string Nombre { get; set; }

    // Contraseña cifrada con la llave ClaveContrasenas de SQL Server, para poder verla desde SQL.
    // El login NO usa esto: sigue usando PasswordHash de Identity.
    public byte[]? PasswordCifrada { get; set; }
}
