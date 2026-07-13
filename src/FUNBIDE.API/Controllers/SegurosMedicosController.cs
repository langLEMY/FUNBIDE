using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Application.UseCases.SegurosMedicos;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Catálogo de aseguradoras (ARS): Caja solo lo consulta para el combo de Cobros; Admin y
/// Lemy lo administran. Cada acción declara su propio <see cref="RequiereRolAttribute"/>
/// en vez de uno a nivel de clase, igual que <c>PacientesController</c>.
/// </summary>
[ApiController]
[Route("api/seguros-medicos")]
[Authorize]
public sealed class SegurosMedicosController(
    IListarSegurosMedicosUseCase listarSeguros,
    ICrearSeguroMedicoUseCase crearSeguro,
    IEditarSeguroMedicoUseCase editarSeguro,
    IDesactivarSeguroMedicoUseCase desactivarSeguro,
    IReactivarSeguroMedicoUseCase reactivarSeguro) : ControllerBase
{
    [HttpGet]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<IReadOnlyList<SeguroMedicoDto>>> ListarAsync(
        [FromQuery] bool incluirInactivos, CancellationToken cancellationToken) =>
        Ok(await listarSeguros.EjecutarAsync(incluirInactivos, cancellationToken));

    [HttpPost]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<SeguroMedicoDto>> CrearAsync(
        CrearSeguroMedicoRequest request, CancellationToken cancellationToken)
    {
        var seguro = await crearSeguro.EjecutarAsync(request, cancellationToken);
        return Created("api/seguros-medicos", seguro);
    }

    [HttpPatch]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<SeguroMedicoDto>> EditarAsync(
        EditarSeguroMedicoRequest request, CancellationToken cancellationToken) =>
        Ok(await editarSeguro.EjecutarAsync(request, cancellationToken));

    [HttpPatch("{id:guid}/desactivar")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<SeguroMedicoDto>> DesactivarAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await desactivarSeguro.EjecutarAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/reactivar")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<SeguroMedicoDto>> ReactivarAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await reactivarSeguro.EjecutarAsync(id, cancellationToken));
}
