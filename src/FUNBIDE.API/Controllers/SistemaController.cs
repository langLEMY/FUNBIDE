using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Application.UseCases.Sistema;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Panel de "Estado del sistema", dentro de Mi Perfil. LEMY lo usa para diagnóstico
/// operativo; Admin lo ve como parte de su visibilidad de solo lectura sobre todo el sistema.
/// </summary>
[ApiController]
[Route("api/sistema")]
[Authorize]
[RequiereRol(RolUsuario.Lemy, RolUsuario.Admin)]
[SoloLecturaEInsercion]
[VisibleDuranteMantenimiento]
public sealed class SistemaController(IVerificarEstadoSistemaUseCase verificarEstado) : ControllerBase
{
    [HttpGet("estado")]
    public async Task<ActionResult<EstadoSistemaDto>> ObtenerEstadoAsync(CancellationToken cancellationToken) =>
        Ok(await verificarEstado.EjecutarAsync(cancellationToken));
}
