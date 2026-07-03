using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Auditoria;
using FUNBIDE.Application.UseCases.Auditoria;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Acceso de solo lectura a los logs de auditoría para ADMIN. Los registros se
/// insertan automáticamente vía el sink de Serilog (ver <c>SerilogConfiguration</c>);
/// este controlador es append-only por si en el futuro se necesita registrar un
/// evento manual, pero nunca permite editar ni borrar auditoría histórica.
/// </summary>
[ApiController]
[Route("api/auditoria")]
[Authorize]
[RequiereRol(RolUsuario.Admin)]
[SoloLecturaEInsercion]
public sealed class AuditoriaController(IObtenerLogsAuditoriaUseCase obtenerLogs) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditoriaLogDto>>> ObtenerAsync(
        [FromQuery] DateTimeOffset? desde,
        [FromQuery] DateTimeOffset? hasta,
        [FromQuery] string? recurso,
        CancellationToken cancellationToken) =>
        Ok(await obtenerLogs.EjecutarAsync(new ConsultarAuditoriaRequest(desde, hasta, recurso), cancellationToken));
}
