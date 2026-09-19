using Microsoft.EntityFrameworkCore;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Data;
using PechugaVainilla.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<PechugaVainillaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// AddScoped: una instancia nueva de cada servicio por peticion HTTP.
// Aqui conectamos la interfaz (lo que pide el controller) con su implementacion real (Infrastructure).
builder.Services.AddScoped<IVendedorService, VendedorService>();
builder.Services.AddScoped<IProductoService, ProductoService>();

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
}

app.UseHttpsRedirection();

// Sirve el Angular compilado (client/dist -> wwwroot). UseDefaultFiles busca index.html
// como pagina inicial; UseStaticFiles sirve el resto (JS, CSS, imagenes).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Cualquier ruta que no sea /api/... ni un archivo estatico existente, regresa index.html.
// Necesario porque Angular Router maneja las rutas en el navegador (no existen como archivos
// reales en el servidor) - sin esto, recargar la pagina en una ruta como /catalogo daria 404.
app.MapFallbackToFile("index.html");

app.Run();
