using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Dashboard;
using FUNBIDE.Application.UseCases.Dashboard;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Métricas agregadas para el panel de ADMIN: tarjetas del día y la serie mensual que
/// alimenta el gráfico "Vista del mes". Ambas se leen de <c>ResumenDiario</c>, que
/// otros casos de uso (citas completadas, movimientos financieros) van acumulando.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
[RequiereRol(RolUsuario.Admin)]
public sealed class DashboardController(
    IObtenerResumenHoyUseCase obtenerResumenHoy,
    IObtenerResumenMesUseCase obtenerResumenMes) : ControllerBase
{
    [HttpGet("resumen-hoy")]
    public async Task<ActionResult<ResumenDiarioDto>> ObtenerResumenHoyAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerResumenHoy.EjecutarAsync(cancellationToken));

    [HttpGet("resumen-mes")]
    public async Task<ActionResult<IReadOnlyList<ResumenDiarioDto>>> ObtenerResumenMesAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerResumenMes.EjecutarAsync(cancellationToken));
}
