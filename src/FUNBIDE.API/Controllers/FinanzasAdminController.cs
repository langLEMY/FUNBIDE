using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.FinanzasAdmin;
using FUNBIDE.Application.UseCases.FinanzasAdmin;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Vista consolidada de finanzas para Admin: todos los movimientos importantes (cobros +
/// movimientos financieros) y el gráfico de ganancias anuales. El registro de gastos es
/// append-only, igual que Finanzas/Cobros de Caja — de ahí <see cref="SoloLecturaEInsercionAttribute"/>.
/// </summary>
[ApiController]
[Route("api/finanzas-admin")]
[Authorize]
[SoloLecturaEInsercion]
public sealed class FinanzasAdminController(
    IListarMovimientosImportantesUseCase listarMovimientos,
    IObtenerResumenAnualUseCase obtenerResumenAnual,
    IObtenerResumenPorPeriodoUseCase obtenerResumenPorPeriodo,
    IRegistrarGastoAdminUseCase registrarGasto) : ControllerBase
{
    [HttpGet("movimientos")]
    [RequierePermiso(ModuloPermiso.Finanzas, ModuloPermiso.Resumen)]
    public async Task<ActionResult<IReadOnlyList<MovimientoImportanteDto>>> ListarMovimientosAsync(
        [FromQuery] DateTimeOffset desde, [FromQuery] DateTimeOffset hasta, CancellationToken cancellationToken) =>
        Ok(await listarMovimientos.EjecutarAsync(new ListarMovimientosImportantesRequest(desde, hasta), cancellationToken));

    [HttpGet("resumen-anual")]
    [RequierePermiso(ModuloPermiso.Finanzas, ModuloPermiso.Gastos)]
    public async Task<ActionResult<IReadOnlyList<ResumenMensualDto>>> ObtenerResumenAnualAsync(
        [FromQuery] int anio, CancellationToken cancellationToken) =>
        Ok(await obtenerResumenAnual.EjecutarAsync(anio, cancellationToken));

    // Hermano de resumen-anual con granularidad diaria/semanal (ver ObtenerResumenPorPeriodoUseCase)
    // para el selector de tendencia del gráfico — pensado para un rango acotado (un mes), no
    // para un año entero con granularidad diaria (365 puntos).
    [HttpGet("resumen-periodo")]
    [RequierePermiso(ModuloPermiso.Finanzas, ModuloPermiso.Gastos)]
    public async Task<ActionResult<IReadOnlyList<ResumenPeriodoDto>>> ObtenerResumenPorPeriodoAsync(
        [FromQuery] DateTimeOffset desde, [FromQuery] DateTimeOffset hasta,
        [FromQuery] GranularidadResumen granularidad, CancellationToken cancellationToken) =>
        Ok(await obtenerResumenPorPeriodo.EjecutarAsync(
            new ObtenerResumenPorPeriodoRequest(desde, hasta, granularidad), cancellationToken));

    [HttpPost("gastos")]
    [RequierePermiso(ModuloPermiso.Gastos)]
    public async Task<ActionResult<MovimientoImportanteDto>> RegistrarGastoAsync(
        RegistrarGastoAdminRequest request, CancellationToken cancellationToken)
    {
        var gasto = await registrarGasto.EjecutarAsync(request, cancellationToken);
        return Created("api/finanzas-admin/movimientos", gasto);
    }
}
