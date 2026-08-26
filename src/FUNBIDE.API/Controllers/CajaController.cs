using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Application.UseCases.Caja;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Turno de caja (apertura/cierre con arqueo) y el balance del dashboard de Caja/Recepción.
/// Abrir/cerrar/ver el resumen del turno actual y operar la caja es de Fondos (la cajera)
/// y de Admin (supervisión/respaldo) — el dinero de la fundación (Caja, Cobros, Finanzas,
/// Gastos, Donaciones) siempre pasa por Admin de una forma u otra. El historial de turnos
/// (<see cref="ListarTurnosAsync"/>) es de solo lectura para Admin. Cada acción declara su
/// propio <see cref="RequiereRolAttribute"/> en vez de uno a nivel de clase, igual que
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
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Caja, ModuloPermiso.Cobros)]
    public async Task<ActionResult<TurnoCajaDto?>> ObtenerTurnoActualAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerTurnoActual.EjecutarAsync(cancellationToken));

    [HttpPost("turnos")]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Caja)]
    public async Task<ActionResult<TurnoCajaDto>> AbrirTurnoAsync(
        AbrirTurnoCajaRequest request, CancellationToken cancellationToken)
    {
        var turno = await abrirTurno.EjecutarAsync(request, cancellationToken);
        return Created("api/caja/turnos/actual", turno);
    }

    [HttpPatch("turnos/cerrar")]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Caja)]
    public async Task<ActionResult<TurnoCajaDto>> CerrarTurnoAsync(
        CerrarTurnoCajaRequest request, CancellationToken cancellationToken) =>
        Ok(await cerrarTurno.EjecutarAsync(request, cancellationToken));

    [HttpGet("resumen")]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Caja)]
    public async Task<ActionResult<ResumenCajaDto>> ObtenerResumenAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerResumen.EjecutarAsync(cancellationToken));

    [HttpGet("turnos")]
    [RequiereRol(RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Operaciones)]
    public async Task<ActionResult<IReadOnlyList<TurnoCajaAdminDto>>> ListarTurnosAsync(
        [FromQuery] DateTimeOffset desde, [FromQuery] DateTimeOffset hasta, CancellationToken cancellationToken) =>
        Ok(await listarTurnos.EjecutarAsync(new ListarTurnosCajaRequest(desde, hasta), cancellationToken));
}
