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
/// lectura (GET) e inserción (POST). Admin puede leer (supervisión de solo lectura) pero
/// no registrar entradas: eso sigue siendo exclusivo del Doctor. Cada acción declara su
/// propio <see cref="RequiereRolAttribute"/> en vez de uno a nivel de clase.
/// </summary>
[ApiController]
[Route("api/historial-clinico")]
[Authorize]
[SoloLecturaEInsercion]
public sealed class HistorialClinicoController(
    IRegistrarEntradaHistorialUseCase registrarEntrada,
    IObtenerHistorialPorPacienteUseCase obtenerPorPaciente) : ControllerBase
{
    [HttpGet("paciente/{pacienteId:guid}")]
    [RequiereRol(RolUsuario.Doctor, RolUsuario.Admin)]
    public async Task<ActionResult<IReadOnlyList<EntradaHistorialDto>>> ObtenerPorPacienteAsync(
        Guid pacienteId, CancellationToken cancellationToken) =>
        Ok(await obtenerPorPaciente.EjecutarAsync(pacienteId, cancellationToken));

    [HttpPost]
    [RequiereRol(RolUsuario.Doctor)]
    public async Task<ActionResult<EntradaHistorialDto>> RegistrarAsync(
        RegistrarEntradaHistorialRequest request, CancellationToken cancellationToken)
    {
        var entrada = await registrarEntrada.EjecutarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorPacienteAsync), new { pacienteId = entrada.PacienteId }, entrada);
    }
}
