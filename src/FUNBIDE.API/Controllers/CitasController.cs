using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Application.UseCases.Citas;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Gestión de citas del doctor autenticado: pendientes, programadas y completadas.
/// </summary>
[ApiController]
[Route("api/citas")]
[Authorize]
[RequiereRol(RolUsuario.Doctor)]
public sealed class CitasController(
    IObtenerCitasPorEstadoUseCase obtenerPorEstado,
    ICrearCitaUseCase crearCita,
    IProgramarCitaUseCase programarCita,
    ICompletarCitaUseCase completarCita) : ControllerBase
{
    [HttpGet("pendientes")]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> ObtenerPendientesAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerPorEstado.EjecutarAsync(EstadoCita.Pendiente, cancellationToken));

    [HttpGet("programadas")]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> ObtenerProgramadasAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerPorEstado.EjecutarAsync(EstadoCita.Programada, cancellationToken));

    [HttpGet("completadas")]
    public async Task<ActionResult<IReadOnlyList<CitaDto>>> ObtenerCompletadasAsync(CancellationToken cancellationToken) =>
        Ok(await obtenerPorEstado.EjecutarAsync(EstadoCita.Completada, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CitaDto>> CrearAsync(CrearCitaRequest request, CancellationToken cancellationToken)
    {
        var cita = await crearCita.EjecutarAsync(request, cancellationToken);
        return Created("api/citas/pendientes", cita);
    }

    [HttpPatch("programar")]
    public async Task<ActionResult<CitaDto>> ProgramarAsync(ProgramarCitaRequest request, CancellationToken cancellationToken) =>
        Ok(await programarCita.EjecutarAsync(request, cancellationToken));

    [HttpPatch("completar")]
    public async Task<ActionResult<CitaDto>> CompletarAsync(CompletarCitaRequest request, CancellationToken cancellationToken) =>
        Ok(await completarCita.EjecutarAsync(request, cancellationToken));
}
