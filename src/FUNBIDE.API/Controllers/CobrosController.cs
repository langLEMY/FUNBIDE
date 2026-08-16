using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Cobros;
using FUNBIDE.Application.UseCases.Cobros;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Cobros a pacientes: comprobante inmutable, igual que Finanzas — append-only, cada
/// cobro alimenta el desglose y el arqueo del turno de caja actual.
/// </summary>
[ApiController]
[Route("api/cobros")]
[Authorize]
[RequierePermiso(ModuloPermiso.Cobros)]
[SoloLecturaEInsercion]
public sealed class CobrosController(
    IRegistrarCobroUseCase registrarCobro,
    IListarCobrosDelTurnoUseCase listarCobrosDelTurno,
    IObtenerDeudaPacienteUseCase obtenerDeudaPaciente) : ControllerBase
{
    [HttpGet("turno-actual")]
    public async Task<ActionResult<IReadOnlyList<CobroDto>>> ListarDelTurnoActualAsync(CancellationToken cancellationToken) =>
        Ok(await listarCobrosDelTurno.EjecutarAsync(cancellationToken));

    [HttpGet("deuda/{pacienteId:guid}")]
    public async Task<ActionResult<DeudaPacienteDto>> ObtenerDeudaAsync(
        Guid pacienteId, CancellationToken cancellationToken) =>
        Ok(await obtenerDeudaPaciente.EjecutarAsync(pacienteId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CobroDto>> RegistrarAsync(
        RegistrarCobroRequest request, CancellationToken cancellationToken)
    {
        var cobro = await registrarCobro.EjecutarAsync(request, cancellationToken);
        return Created("api/cobros/turno-actual", cobro);
    }
}
