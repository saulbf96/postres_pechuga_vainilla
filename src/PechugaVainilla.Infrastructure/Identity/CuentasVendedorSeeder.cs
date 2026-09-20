using Microsoft.AspNetCore.Identity;
using PechugaVainilla.Infrastructure.Data;

namespace PechugaVainilla.Infrastructure.Identity;

// Crea las cuentas de ejemplo para entrar al panel y las vincula a su Vendedor.
// OJO: contraseña de ejemplo, obviamente falsa - hay que cambiarla antes de usar esto de verdad
// (igual que el WhatsApp y los precios ficticios del catalogo).
public static class CuentasVendedorSeeder
{
    private const string PasswordEjemplo = "CambiaEsto123";

    public static async Task SeedAsync(UserManager<Usuario> userManager, PechugaVainillaDbContext context)
    {
        await CrearCuentaSiNoExisteAsync(userManager, context, "vainilla@pechugayvainilla.com", "Administradora de Vainilla", "vainilla");
        await CrearCuentaSiNoExisteAsync(userManager, context, "pechuga@pechugayvainilla.com", "Administradora de Pechuga", "pechuga");
    }

    private static async Task CrearCuentaSiNoExisteAsync(
        UserManager<Usuario> userManager,
        PechugaVainillaDbContext context,
        string email,
        string nombre,
        string slugVendedor)
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

        if (!await userManager.IsInRoleAsync(usuario, "Vendedor"))
        {
            await userManager.AddToRoleAsync(usuario, "Vendedor");
        }

        var vendedor = context.Vendedores.FirstOrDefault(v => v.Slug == slugVendedor);
        if (vendedor is not null && vendedor.UsuarioId is null)
        {
            vendedor.UsuarioId = usuario.Id;
            context.SaveChanges();
        }
    }
}
