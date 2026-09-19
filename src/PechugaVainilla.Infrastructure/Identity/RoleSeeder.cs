using Microsoft.AspNetCore.Identity;

namespace PechugaVainilla.Infrastructure.Identity;

public static class RoleSeeder
{
    public static readonly string[] Roles = ["Administrador", "Vendedor", "Cliente", "Soporte"];

    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var rol in Roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }
    }
}
