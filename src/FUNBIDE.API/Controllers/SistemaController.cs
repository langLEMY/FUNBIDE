using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Application.UseCases.Sistema;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Panel de "Estado del sistema" exclusivo de LEMY, dentro de Mi Perfil.
/// </summary>
[ApiController]
[Route("api/sistema")]
[Authorize]
[RequiereRol(RolUsuario.Lemy)]
[SoloLecturaEInsercion]
public sealed class SistemaController(IVerificarEstadoSistemaUseCase verificarEstado) : ControllerBase
{
    [HttpGet("estado")]
    public async Task<ActionResult<EstadoSistemaDto>> ObtenerEstadoAsync(CancellationToken cancellationToken) =>
        Ok(await verificarEstado.EjecutarAsync(cancellationToken));
}
