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
[RequiereRol(RolUsuario.Admin)]
[SoloLecturaEInsercion]
public sealed class FinanzasAdminController(
    IListarMovimientosImportantesUseCase listarMovimientos,
    IObtenerResumenAnualUseCase obtenerResumenAnual,
    IRegistrarGastoAdminUseCase registrarGasto) : ControllerBase
{
    [HttpGet("movimientos")]
    public async Task<ActionResult<IReadOnlyList<MovimientoImportanteDto>>> ListarMovimientosAsync(
        [FromQuery] DateTimeOffset desde, [FromQuery] DateTimeOffset hasta, CancellationToken cancellationToken) =>
        Ok(await listarMovimientos.EjecutarAsync(new ListarMovimientosImportantesRequest(desde, hasta), cancellationToken));

    [HttpGet("resumen-anual")]
    public async Task<ActionResult<IReadOnlyList<ResumenMensualDto>>> ObtenerResumenAnualAsync(
        [FromQuery] int anio, CancellationToken cancellationToken) =>
        Ok(await obtenerResumenAnual.EjecutarAsync(anio, cancellationToken));

    [HttpPost("gastos")]
    public async Task<ActionResult<MovimientoImportanteDto>> RegistrarGastoAsync(
        RegistrarGastoAdminRequest request, CancellationToken cancellationToken)
    {
        var gasto = await registrarGasto.EjecutarAsync(request, cancellationToken);
        return Created("api/finanzas-admin/movimientos", gasto);
    }
}
