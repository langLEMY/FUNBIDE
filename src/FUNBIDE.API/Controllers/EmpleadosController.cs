using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Empleados;
using FUNBIDE.Application.UseCases.Empleados;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Directorio simple del personal del hospital (nombre y cargo, sin login). ADMIN
/// solo puede consultarlo; LEMY es quien lo administra. Cada acción declara su propio
/// <see cref="RequiereRolAttribute"/> en vez de uno a nivel de clase para que quede
/// explícito qué rol puede hacer qué, sin depender de reglas de precedencia.
/// </summary>
[ApiController]
[Route("api/empleados")]
[Authorize]
public sealed class EmpleadosController(
    IListarEmpleadosUseCase listarEmpleados,
    ICrearEmpleadoUseCase crearEmpleado,
    IEditarEmpleadoUseCase editarEmpleado,
    IEliminarEmpleadoUseCase eliminarEmpleado) : ControllerBase
{
    [HttpGet]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<IReadOnlyList<EmpleadoDto>>> ListarAsync(CancellationToken cancellationToken) =>
        Ok(await listarEmpleados.EjecutarAsync(cancellationToken));

    [HttpPost]
    [RequiereRol(RolUsuario.Lemy)]
    public async Task<ActionResult<EmpleadoDto>> CrearAsync(
        CrearEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var empleado = await crearEmpleado.EjecutarAsync(request, cancellationToken);
        return Created("api/empleados", empleado);
    }

    [HttpPatch]
    [RequiereRol(RolUsuario.Lemy)]
    public async Task<ActionResult<EmpleadoDto>> EditarAsync(
        EditarEmpleadoRequest request, CancellationToken cancellationToken) =>
        Ok(await editarEmpleado.EjecutarAsync(request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [RequiereRol(RolUsuario.Lemy)]
    public async Task<IActionResult> EliminarAsync(Guid id, CancellationToken cancellationToken)
    {
        await eliminarEmpleado.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
