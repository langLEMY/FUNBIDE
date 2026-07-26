using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Application.UseCases.Sistema;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Prender/apagar el modo mantenimiento (ver <c>MantenimientoMiddleware</c>). Separado de
/// <see cref="SistemaController"/> a propósito: ese controlador está marcado
/// <c>[SoloLecturaEInsercion]</c> deliberadamente para que ninguna acción de escritura se
/// le agregue nunca por error, así que la escritura real vive acá.
/// </summary>
[ApiController]
[Route("api/mantenimiento")]
[Authorize]
public sealed class MantenimientoController(ICambiarModoMantenimientoUseCase cambiarModoMantenimiento) : ControllerBase
{
    [HttpPatch]
    [RequiereRol(RolUsuario.Lemy)]
    public async Task<ActionResult<ModoMantenimientoDto>> CambiarAsync(
        CambiarModoMantenimientoRequest request, CancellationToken cancellationToken) =>
        Ok(await cambiarModoMantenimiento.EjecutarAsync(request, cancellationToken));
}
