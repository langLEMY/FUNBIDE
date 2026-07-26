using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Application.UseCases.Caja;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Turno de caja (apertura/cierre con arqueo) y el balance del dashboard de Caja/Recepción.
/// Abrir/cerrar/ver el resumen del turno actual sigue siendo exclusivo de Fondos; el
/// historial de turnos (<see cref="ListarTurnosAsync"/>) es de solo lectura para Admin,
/// que supervisa pero no opera la caja. Cada acción declara su propio
/// <see cref="RequiereRolAttribute"/> en vez de uno a nivel de clase, igual que
/// <c>CitasController</c>/<c>PacientesController</c>.
/// </summary>
[ApiController]
[Route("api/caja")]
[Authorize]
public sealed class CajaController(
    IAbrirTurnoCajaUseCase abrirTurno,
    ICerrarTurnoCajaUseCase cerrarTurno,
    IObtenerTurnoCajaActualUseCase obtenerTurnoActual,
    IObtenerResumenCajaUseCase obtenerResumen,
    IListarTurnosCajaUseCase listarTurnos) : ControllerBase
{
    [HttpGet("turnos/actual")]
    [RequiereRol(RolUsuario.Fondos)]
    public async Task<ActionResult<TurnoCajaDto?>> ObtenerTurnoActualAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerTurnoActual.EjecutarAsync(cancellationToken));

    [HttpPost("turnos")]
    [RequiereRol(RolUsuario.Fondos)]
    public async Task<ActionResult<TurnoCajaDto>> AbrirTurnoAsync(
        AbrirTurnoCajaRequest request, CancellationToken cancellationToken)
    {
        var turno = await abrirTurno.EjecutarAsync(request, cancellationToken);
        return Created("api/caja/turnos/actual", turno);
    }

    [HttpPatch("turnos/cerrar")]
    [RequiereRol(RolUsuario.Fondos)]
    public async Task<ActionResult<TurnoCajaDto>> CerrarTurnoAsync(
        CerrarTurnoCajaRequest request, CancellationToken cancellationToken) =>
        Ok(await cerrarTurno.EjecutarAsync(request, cancellationToken));

    [HttpGet("resumen")]
    [RequiereRol(RolUsuario.Fondos)]
    public async Task<ActionResult<ResumenCajaDto>> ObtenerResumenAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerResumen.EjecutarAsync(cancellationToken));

    [HttpGet("turnos")]
    [RequiereRol(RolUsuario.Admin)]
    public async Task<ActionResult<IReadOnlyList<TurnoCajaAdminDto>>> ListarTurnosAsync(
        [FromQuery] DateTimeOffset desde, [FromQuery] DateTimeOffset hasta, CancellationToken cancellationToken) =>
        Ok(await listarTurnos.EjecutarAsync(new ListarTurnosCajaRequest(desde, hasta), cancellationToken));
}
