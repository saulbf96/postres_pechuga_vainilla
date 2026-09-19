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

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PechugaVainillaDbContext>();
    DbSeeder.Seed(db);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
