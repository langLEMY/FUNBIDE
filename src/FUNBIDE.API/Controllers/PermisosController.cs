using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Permisos;
using FUNBIDE.Application.UseCases.Permisos;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Administra qué módulos togglables (<see cref="ModuloPermiso"/>) puede ver cada
/// miembro del personal, por encima del default de su rol. Gateado siempre con
/// <see cref="RequiereRolAttribute"/> fijo — nunca <see cref="RequierePermisoAttribute"/>
/// — porque esta es justamente la herramienta que administra el resto de los permisos:
/// no puede depender de sí misma.
/// </summary>
[ApiController]
[Route("api/permisos")]
[Authorize]
[RequiereRol(RolUsuario.Lemy, RolUsuario.Admin)]
public sealed class PermisosController(
    IObtenerPermisosDeUsuarioUseCase obtenerPermisos,
    IActualizarPermisosDeUsuarioUseCase actualizarPermisos) : ControllerBase
{
    [HttpGet("{usuarioId:guid}")]
    public async Task<ActionResult<PermisosUsuarioDto>> ObtenerAsync(Guid usuarioId, CancellationToken cancellationToken) =>
        Ok(await obtenerPermisos.EjecutarAsync(usuarioId, cancellationToken));

    [HttpPut]
    public async Task<ActionResult<PermisosUsuarioDto>> ActualizarAsync(
        ActualizarPermisosDeUsuarioRequest request, CancellationToken cancellationToken) =>
        Ok(await actualizarPermisos.EjecutarAsync(request, cancellationToken));
}
