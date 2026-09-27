using Microsoft.AspNetCore.Identity;

namespace PechugaVainilla.Infrastructure.Identity;

// Crea las cuentas de ejemplo para entrar al panel: los dos administradores del negocio.
// OJO: contraseña de ejemplo, obviamente falsa - hay que cambiarla antes de usar esto de verdad
// (igual que el WhatsApp y los precios ficticios del catalogo).
public static class CuentasAdministradorSeeder
{
    private const string PasswordEjemplo = "CambiaEsto123";

    public static async Task SeedAsync(UserManager<Usuario> userManager)
    {
        await CrearCuentaSiNoExisteAsync(userManager, "vainilla@pechugayvainilla.com", "Administradora 1");
        await CrearCuentaSiNoExisteAsync(userManager, "pechuga@pechugayvainilla.com", "Administradora 2");
    }

    private static async Task CrearCuentaSiNoExisteAsync(UserManager<Usuario> userManager, string email, string nombre)
    {
        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario is null)
        {
            usuario = new Usuario { UserName = email, Email = email, Nombre = nombre, EmailConfirmed = true };
            var resultado = await userManager.CreateAsync(usuario, PasswordEjemplo);
            if (!resultado.Succeeded)
            {
                return; // no deberia pasar con datos fijos, pero no tumbamos el arranque de la app por esto
            }
        }

        if (!await userManager.IsInRoleAsync(usuario, "Administrador"))
        {
            await userManager.AddToRoleAsync(usuario, "Administrador");
        }
    }
}
