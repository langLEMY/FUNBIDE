using FUNBIDE.API.Authorization;
using FUNBIDE.API.Extensions;
using FUNBIDE.Application.DTOs.SegurosMedicos;
using FUNBIDE.Application.UseCases.SegurosMedicos;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Tarifario por procedimiento de una aseguradora (hoy solo SENASA, con sus 3 planes).
/// Listar es lo que alimenta el buscador de procedimientos de Cobros — mismo permiso que
/// Aseguradoras, porque conceptualmente es parte de ese catálogo.
/// </summary>
[ApiController]
[Route("api/tarifario-procedimientos")]
[Authorize]
[RequierePermiso(ModuloPermiso.Aseguradoras)]
public sealed class TarifarioProcedimientosController(
    IListarTarifarioUseCase listarTarifario,
    IImportarTarifarioUseCase importarTarifario) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TarifarioProcedimientoDto>>> ListarAsync(
        [FromQuery] Guid seguroMedicoId, [FromQuery] PlanSenasa plan, CancellationToken cancellationToken) =>
        Ok(await listarTarifario.EjecutarAsync(new ListarTarifarioRequest(seguroMedicoId, plan), cancellationToken));

    [HttpPost("importar")]
    [RequiereRol(RolUsuario.Lemy, RolUsuario.Admin)]
    public async Task<ActionResult<ImportarTarifarioResultDto>> ImportarAsync(
        [FromQuery] Guid seguroMedicoId, [FromQuery] PlanSenasa plan, IFormFile archivo, CancellationToken cancellationToken)
    {
        if (!archivo.EsExcelValido(out var error))
        {
            return BadRequest(new { titulo = "Solicitud inválida", detalle = error });
        }

        await using var contenido = archivo.OpenReadStream();
        return Ok(await importarTarifario.EjecutarAsync(
            new ImportarTarifarioRequest(seguroMedicoId, plan, contenido), cancellationToken));
    }
}
