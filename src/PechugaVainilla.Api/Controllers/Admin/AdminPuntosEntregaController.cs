using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PechugaVainilla.Api.Dtos;
using PechugaVainilla.Core.Entities;
using PechugaVainilla.Core.Enums;
using PechugaVainilla.Core.Interfaces;
using PechugaVainilla.Infrastructure.Identity;

namespace PechugaVainilla.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/puntos-entrega")]
[Authorize(Roles = "Administrador,Vendedor")]
public class AdminPuntosEntregaController : ControllerBase
{
    private readonly IPuntoEntregaService _puntoEntregaService;
    private readonly IAccesoVendedorService _accesoVendedorService;
    private readonly UserManager<Usuario> _userManager;

    public AdminPuntosEntregaController(IPuntoEntregaService puntoEntregaService, IAccesoVendedorService accesoVendedorService, UserManager<Usuario> userManager)
    {
        _puntoEntregaService = puntoEntregaService;
        _accesoVendedorService = accesoVendedorService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPuntoEntregaDto>>> Get(CancellationToken cancellationToken)
    {
        var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
        var puntos = await _puntoEntregaService.ObtenerTodosAsync(vendedorIds, cancellationToken);
        return Ok(puntos.Select(MapearDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<AdminPuntoEntregaDto>> Crear(GuardarPuntoEntregaRequest request, CancellationToken cancellationToken)
    {
        var vendedorId = await ResolverVendedorIdParaEscrituraAsync(request.VendedorId, cancellationToken);
        if (vendedorId is null)
        {
            return BadRequest(new { mensaje = "Indica a que vendedor pertenece el punto de entrega." });
        }

        if (!TryMapearDatos(request, out var datos, out var error))
        {
            return BadRequest(new { mensaje = error });
        }

        try
        {
            var punto = await _puntoEntregaService.CrearAsync(vendedorId.Value, datos, cancellationToken);
            return Ok(MapearDto(punto));
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, GuardarPuntoEntregaRequest request, CancellationToken cancellationToken)
    {
        if (!TryMapearDatos(request, out var datos, out var error))
        {
            return BadRequest(new { mensaje = error });
        }

        try
        {
            var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
            await _puntoEntregaService.ActualizarAsync(id, vendedorIds, datos, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("{id:int}/activo")]
    public async Task<IActionResult> CambiarActivo(int id, CambiarActivoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var vendedorIds = await ResolverVendedorIdsAsync(cancellationToken);
            await _puntoEntregaService.CambiarActivoAsync(id, vendedorIds, request.Activo, cancellationToken);
            return NoContent();
        }
        catch (ReglaDeNegocioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private async Task<IReadOnlyList<int>?> ResolverVendedorIdsAsync(CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrador"))
        {
            return null;
        }

        var usuarioId = _userManager.GetUserId(User)!;
        return await _accesoVendedorService.ObtenerVendedorIdsAsync(usuarioId, cancellationToken);
    }

    private async Task<int?> ResolverVendedorIdParaEscrituraAsync(int? vendedorIdSolicitado, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrador"))
        {
            return vendedorIdSolicitado;
        }

        var usuarioId = _userManager.GetUserId(User)!;
        var permitidos = await _accesoVendedorService.ObtenerVendedorIdsAsync(usuarioId, cancellationToken);

        if (vendedorIdSolicitado.HasValue && permitidos.Contains(vendedorIdSolicitado.Value))
        {
            return vendedorIdSolicitado;
        }

        return permitidos.Count == 1 ? permitidos[0] : null;
    }

    private static bool TryMapearDatos(GuardarPuntoEntregaRequest request, out DatosPuntoEntrega datos, out string? error)
    {
        datos = null!;
        error = null;

        var dias = DiasSemana.Ninguno;
        foreach (var nombreDia in request.DiasSemana)
        {
            if (!Enum.TryParse<DiasSemana>(nombreDia, ignoreCase: true, out var dia))
            {
                error = $"'{nombreDia}' no es un día válido.";
                return false;
            }
            dias |= dia;
        }

        if (!TimeOnly.TryParse(request.HoraInicio, out var horaInicio) ||
            !TimeOnly.TryParse(request.HoraFin, out var horaFin) ||
            !TimeOnly.TryParse(request.HoraLimitePedido, out var horaLimite))
        {
            error = "Alguna de las horas no tiene un formato válido (HH:mm).";
            return false;
        }

        datos = new DatosPuntoEntrega(request.Nombre, request.Tipo, dias, horaInicio, horaFin, request.DiasAnticipacion, horaLimite, request.CostoEnvio);
        return true;
    }

    private static List<string> DesglosarDias(DiasSemana dias)
    {
        var resultado = new List<string>();
        foreach (var dia in Enum.GetValues<DiasSemana>())
        {
            if (dia != DiasSemana.Ninguno && dias.HasFlag(dia))
            {
                resultado.Add(dia.ToString());
            }
        }
        return resultado;
    }

    private static AdminPuntoEntregaDto MapearDto(PuntoEntrega punto) => new(
        punto.Id,
        punto.VendedorId,
        punto.Vendedor.Nombre,
        punto.Nombre,
        punto.Tipo.ToString(),
        DesglosarDias(punto.DiasSemana),
        punto.HoraInicio.ToString("HH:mm"),
        punto.HoraFin.ToString("HH:mm"),
        punto.DiasAnticipacion,
        punto.HoraLimitePedido.ToString("HH:mm"),
        punto.CostoEnvio,
        punto.Activo);
}
