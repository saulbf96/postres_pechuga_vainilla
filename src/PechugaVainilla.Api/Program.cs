using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;
using PechugaVainilla.Infrastructure.Identity;
using PechugaVainilla.Infrastructure.Services;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Los enums viajan como texto ("Efectivo", no "3") - mas legible en el JSON
        // y consistente con como se guardan en la base de datos (HasConversion<string>()).
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PechugaVainillaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// AddScoped: una instancia nueva de cada servicio por peticion HTTP.
// Aqui conectamos la interfaz (lo que pide el controller) con su implementacion real (Infrastructure).
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IPuntoEntregaService, PuntoEntregaService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IPerfilesService, PerfilesService>();
builder.Services.AddScoped<CifradoContrasena>();

// Identity: maneja el hash de contraseñas, bloqueo por intentos fallidos, roles, etc.
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    // Politica de contraseñas (CLAUDE.md: "politica de contraseñas, bloqueo por intentos fallidos")
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false; // Identity la activa por default si no se apaga aqui
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<PechugaVainillaDbContext>()
    .AddDefaultTokenProviders()
    .AddErrorDescriber<ErrorDescriberEnEspanol>(); // mensajes de Identity en español, no en ingles

// Decidimos cookie HttpOnly (no JWT/localStorage) porque Angular y la API viven en el mismo
// sitio - evita el riesgo de robo de token por XSS que tendria guardarlo en localStorage.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;

    // Somos una API: si no hay sesion, regresamos 401/403 en vez de redirigir a una pagina HTML de login.
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Swagger UI: pagina interactiva en /swagger que lee el documento OpenAPI de arriba.
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Pechuga y Vainilla API v1");
    });

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PechugaVainillaDbContext>();
    DbSeeder.Seed(db);

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
    await CuentasAdministradorSeeder.SeedAsync(userManager);

    // "dotnet run" desde la terminal NO abre el navegador solo (eso solo lo hace Visual Studio
    // con F5) - lo abrimos nosotros mismos en cuanto el servidor ya esta listo para recibir peticiones.
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var url = app.Urls.FirstOrDefault() ?? "http://localhost:5081";
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Si por lo que sea no se puede abrir el navegador, no tumbamos la app por eso.
        }
    });
}

app.UseHttpsRedirection();

// Sirve el Angular compilado (client/dist -> wwwroot). UseDefaultFiles busca index.html
// como pagina inicial; UseStaticFiles sirve el resto (JS, CSS, imagenes).
app.UseDefaultFiles();
app.UseStaticFiles();

// UseAuthentication identifica QUIEN es el usuario (lee la cookie); UseAuthorization decide
// QUE puede hacer. Van en ese orden y ambas antes de MapControllers.
app.UseAuthentication();
app.UseAuthorization();

// Revisa en cada peticion que requiere sesion:
// - Cuenta desactivada: 401 en todo (aunque la cookie siga viva), para que salga de inmediato.
// - Contraseña expirada/temporal: no puede usar nada (panel, pedidos...) hasta cambiarla.
//   Las rutas /api/v1/auth quedan libres para poder cambiarla o salir.
app.Use(async (context, next) =>
{
    var requiereSesion = context.GetEndpoint()?.Metadata.GetMetadata<IAuthorizeData>() is not null;
    var esRutaDeAuth = context.Request.Path.StartsWithSegments("/api/v1/auth");

    if (requiereSesion && context.User.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<Usuario>>();
        var usuario = await userManager.GetUserAsync(context.User);

        if (usuario is not null && !usuario.Activo)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { mensaje = "Tu cuenta está desactivada." });
            return;
        }

        if (!esRutaDeAuth && usuario?.DebeCambiarPassword == true)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { mensaje = "Tu contraseña expiró. Escribe una contraseña nueva para continuar." });
            return;
        }
    }

    await next();
});

app.MapControllers();

// Cualquier ruta que no sea /api/... ni un archivo estatico existente, regresa index.html.
// Necesario porque Angular Router maneja las rutas en el navegador (no existen como archivos
// reales en el servidor) - sin esto, recargar la pagina en una ruta como /catalogo daria 404.
app.MapFallbackToFile("index.html");

app.Run();
