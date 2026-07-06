using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Finanzas;
using FUNBIDE.Application.UseCases.Finanzas;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Caja de FONDOS: registra ingresos y egresos. Append-only, igual que el historial
/// clínico — cada movimiento es un comprobante inmutable que alimenta el resumen
/// diario del dashboard de ADMIN.
/// </summary>
[ApiController]
[Route("api/finanzas")]
[Authorize]
[RequiereRol(RolUsuario.Fondos)]
[SoloLecturaEInsercion]
public sealed class FinanzasController(
    IRegistrarMovimientoFinancieroUseCase registrarMovimiento,
    IListarMovimientosFinancierosUseCase listarMovimientos) : ControllerBase
{
    [HttpGet("movimientos")]
    public async Task<ActionResult<IReadOnlyList<MovimientoFinancieroDto>>> ListarAsync(
        CancellationToken cancellationToken) =>
        Ok(await listarMovimientos.EjecutarAsync(cancellationToken));

    [HttpPost("movimientos")]
    public async Task<ActionResult<MovimientoFinancieroDto>> RegistrarAsync(
        RegistrarMovimientoFinancieroRequest request, CancellationToken cancellationToken) =>
        Ok(await registrarMovimiento.EjecutarAsync(request, cancellationToken));
}
