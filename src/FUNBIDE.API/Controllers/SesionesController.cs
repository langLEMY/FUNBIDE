using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Sesiones;
using FUNBIDE.Application.UseCases.Sesiones;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Presencia de dispositivos conectados. El latido lo manda cualquier usuario autenticado
/// (todos los roles usan la app desde algún dispositivo); el conteo es solo para el
/// Dashboard de Admin, mismo permiso que <see cref="DashboardController"/>.
/// </summary>
[ApiController]
[Route("api/sesiones")]
[Authorize]
public sealed class SesionesController(
    IRegistrarLatidoUseCase registrarLatido,
    IContarSesionesActivasUseCase contarSesionesActivas) : ControllerBase
{
    [HttpPost("latido")]
    public async Task<ActionResult<LatidoDto>> RegistrarLatidoAsync(
        RegistrarLatidoRequest request, CancellationToken cancellationToken) =>
        Ok(await registrarLatido.EjecutarAsync(request, cancellationToken));

    [HttpGet("activas")]
    [RequierePermiso(ModuloPermiso.Dashboard)]
    public async Task<ActionResult<SesionesActivasDto>> ContarActivasAsync(CancellationToken cancellationToken) =>
        Ok(await contarSesionesActivas.EjecutarAsync(cancellationToken));
}
