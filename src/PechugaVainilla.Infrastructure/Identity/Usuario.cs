using Microsoft.AspNetCore.Identity;

namespace PechugaVainilla.Infrastructure.Identity;

// Extiende IdentityUser (que ya trae Email, PhoneNumber, hash de contraseña, etc.)
// Reutilizamos PhoneNumber para el WhatsApp en vez de duplicar el campo.
// Vive en Infrastructure (no en Core) porque esta acoplado a ASP.NET Core Identity,
// que es una decision de tecnologia, no una entidad pura del negocio.
public class Usuario : IdentityUser
{
    public required string Nombre { get; set; }
}
