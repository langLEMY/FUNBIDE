using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Application.UseCases.Caja;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Turno de caja (cierre con arqueo) y el balance del dashboard de Caja/Recepción. El turno
/// ya no se abre a mano: se abre solo, con <see cref="FUNBIDE.Domain.Entities.TurnoCaja.FondoFijo"/>,
/// en cuanto <c>RegistrarCobroUseCase</c>/<c>RegistrarMovimientoFinancieroUseCase</c> lo
/// necesitan y no hay ninguno abierto. Cerrar/ver el resumen del turno actual y operar la
/// caja es de Fondos (la cajera) y de Admin (supervisión/respaldo) — el dinero de la
/// fundación (Caja, Cobros, Finanzas, Gastos, Donaciones) siempre pasa por Admin de una
/// forma u otra. El historial de turnos (<see cref="ListarTurnosAsync"/>) es de solo lectura
/// para Admin. Cada acción declara su propio <see cref="RequiereRolAttribute"/> en vez de
/// uno a nivel de clase, igual que <c>CitasController</c>/<c>PacientesController</c>.
/// </summary>
[ApiController]
[Route("api/caja")]
[Authorize]
public sealed class CajaController(
    ICerrarTurnoCajaUseCase cerrarTurno,
    IObtenerTurnoCajaActualUseCase obtenerTurnoActual,
    IObtenerResumenCajaUseCase obtenerResumen,
    IListarTurnosCajaUseCase listarTurnos,
    IObtenerReporteCierreCajaUseCase obtenerReporteCierre) : ControllerBase
{
    [HttpGet("turnos/actual")]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Caja, ModuloPermiso.Cobros)]
    public async Task<ActionResult<TurnoCajaDto?>> ObtenerTurnoActualAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerTurnoActual.EjecutarAsync(cancellationToken));

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

    // Mismos roles/permiso que el resto de Caja: quien cerró el turno necesita poder
    // reimprimirlo, y Admin puede reimprimir cualquiera desde el historial.
    [HttpGet("turnos/{id:guid}/reporte-cierre")]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin)]
    [RequierePermiso(ModuloPermiso.Caja)]
    public async Task<ActionResult<ReporteCierreCajaDto>> ObtenerReporteCierreAsync(
        Guid id, CancellationToken cancellationToken) =>
        Ok(await obtenerReporteCierre.EjecutarAsync(id, cancellationToken));
}
