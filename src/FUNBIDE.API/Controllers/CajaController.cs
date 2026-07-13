using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Application.UseCases.Caja;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Turno de caja (apertura/cierre con arqueo) y el balance del dashboard de Caja/Recepción.
/// </summary>
[ApiController]
[Route("api/caja")]
[Authorize]
[RequiereRol(RolUsuario.Fondos)]
public sealed class CajaController(
    IAbrirTurnoCajaUseCase abrirTurno,
    ICerrarTurnoCajaUseCase cerrarTurno,
    IObtenerTurnoCajaActualUseCase obtenerTurnoActual,
    IObtenerResumenCajaUseCase obtenerResumen) : ControllerBase
{
    [HttpGet("turnos/actual")]
    public async Task<ActionResult<TurnoCajaDto?>> ObtenerTurnoActualAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerTurnoActual.EjecutarAsync(cancellationToken));

    [HttpPost("turnos")]
    public async Task<ActionResult<TurnoCajaDto>> AbrirTurnoAsync(
        AbrirTurnoCajaRequest request, CancellationToken cancellationToken)
    {
        var turno = await abrirTurno.EjecutarAsync(request, cancellationToken);
        return Created("api/caja/turnos/actual", turno);
    }

    [HttpPatch("turnos/cerrar")]
    public async Task<ActionResult<TurnoCajaDto>> CerrarTurnoAsync(
        CerrarTurnoCajaRequest request, CancellationToken cancellationToken) =>
        Ok(await cerrarTurno.EjecutarAsync(request, cancellationToken));

    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenCajaDto>> ObtenerResumenAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerResumen.EjecutarAsync(cancellationToken));
}
