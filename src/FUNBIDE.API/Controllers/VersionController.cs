using FUNBIDE.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

public sealed record VersionDto(string Version);

/// <summary>
/// Qué versión de FUNBIDE está corriendo este backend — para el bloque "Acerca de" en Mi
/// Perfil. La versión viene del <c>&lt;Version&gt;</c> del ensamblado (ver
/// FUNBIDE.API.csproj), que a su vez lee <c>version.txt</c> en la raíz del repo — la misma
/// fuente única que usa el launcher para decidir si hay una actualización disponible.
/// Visible para cualquier usuario autenticado (no solo Lemy/Admin, a diferencia de
/// <see cref="SistemaController"/>) y durante el modo mantenimiento, para que el personal
/// pueda confirmar qué versión tiene instalada al reportar un problema.
/// </summary>
[ApiController]
[Route("api/sistema/version")]
[Authorize]
[SoloLecturaEInsercion]
[VisibleDuranteMantenimiento]
public sealed class VersionController : ControllerBase
{
    [HttpGet]
    public ActionResult<VersionDto> Obtener() =>
        Ok(new VersionDto(GetType().Assembly.GetName().Version?.ToString(3) ?? "desconocida"));
}
