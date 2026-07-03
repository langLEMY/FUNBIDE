using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.HistorialClinico;
using FUNBIDE.Application.UseCases.HistorialClinico;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Historial clínico del paciente. Append-only: <see cref="SoloLecturaEInsercionAttribute"/>
/// hace que cualquier PUT/PATCH/DELETE contra este controlador sea rechazado con 403
/// por <c>AppendOnlyGuardMiddleware</c> antes de llegar aquí. Solo existen acciones de
/// lectura (GET) e inserción (POST).
/// </summary>
[ApiController]
[Route("api/historial-clinico")]
[Authorize]
[RequiereRol(RolUsuario.Doctor)]
[SoloLecturaEInsercion]
public sealed class HistorialClinicoController(
    IRegistrarEntradaHistorialUseCase registrarEntrada,
    IObtenerHistorialPorPacienteUseCase obtenerPorPaciente) : ControllerBase
{
    [HttpGet("paciente/{pacienteId:guid}")]
    public async Task<ActionResult<IReadOnlyList<EntradaHistorialDto>>> ObtenerPorPacienteAsync(
        Guid pacienteId, CancellationToken cancellationToken) =>
        Ok(await obtenerPorPaciente.EjecutarAsync(pacienteId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<EntradaHistorialDto>> RegistrarAsync(
        RegistrarEntradaHistorialRequest request, CancellationToken cancellationToken)
    {
        var entrada = await registrarEntrada.EjecutarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorPacienteAsync), new { pacienteId = entrada.PacienteId }, entrada);
    }
}
