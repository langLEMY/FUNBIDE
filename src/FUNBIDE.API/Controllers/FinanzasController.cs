using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Finanzas;
using FUNBIDE.Application.UseCases.Finanzas;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Movimientos de caja (ingresos y egresos) del turno actual. Listar requiere el módulo
/// Caja (FONDOS y ADMIN, ver <c>PermisosPorRolDefault</c>) — Fondos necesita ver el
/// timeline de su propio turno. Registrar un movimiento (siempre un egreso/gasto desde
/// el frontend) requiere en cambio el módulo Gastos, que solo tiene ADMIN: Fondos es un
/// perfil de caja/recepción (agenda, cobra) y no debe autorizar ni registrar gastos de
/// la fundación. Append-only, igual que el historial clínico — cada movimiento es un
/// comprobante inmutable que alimenta el resumen diario del dashboard de ADMIN.
/// </summary>
[ApiController]
[Route("api/finanzas")]
[Authorize]
[RequierePermiso(ModuloPermiso.Caja)]
[SoloLecturaEInsercion]
public sealed class FinanzasController(
    IRegistrarMovimientoFinancieroUseCase registrarMovimiento,
    IListarMovimientosFinancierosUseCase listarMovimientos) : ControllerBase
{
    [HttpGet("movimientos")]
    public async Task<ActionResult<IReadOnlyList<MovimientoFinancieroDto>>> ListarAsync(
        [FromQuery] Guid? turnoCajaId, CancellationToken cancellationToken) =>
        Ok(await listarMovimientos.EjecutarAsync(turnoCajaId, cancellationToken));

    [HttpPost("movimientos")]
    [RequierePermiso(ModuloPermiso.Gastos)]
    public async Task<ActionResult<MovimientoFinancieroDto>> RegistrarAsync(
        RegistrarMovimientoFinancieroRequest request, CancellationToken cancellationToken) =>
        Ok(await registrarMovimiento.EjecutarAsync(request, cancellationToken));
}
