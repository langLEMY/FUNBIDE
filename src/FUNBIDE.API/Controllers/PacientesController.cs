using FUNBIDE.API.Authorization;
using FUNBIDE.API.Extensions;
using FUNBIDE.Application.DTOs.Pacientes;
using FUNBIDE.Application.UseCases.Pacientes;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Base de datos de pacientes (nombre, apellido, cédula, teléfono opcional y foto de
/// cédula opcional). Todos los roles pueden consultar y agregar pacientes; eliminar
/// es de ADMIN, LEMY y DOCTOR; editar y gestionar la foto de cédula sigue siendo
/// exclusivo de LEMY. Cada acción declara su propio <see cref="RequiereRolAttribute"/>
/// en vez de uno a nivel de clase, igual que <c>EmpleadosController</c>.
/// </summary>
[ApiController]
[Route("api/pacientes")]
[Authorize]
public sealed class PacientesController(
    IListarPacientesUseCase listarPacientes,
    IObtenerPacientePorIdUseCase obtenerPacientePorId,
    ICrearPacienteUseCase crearPaciente,
    IEditarPacienteUseCase editarPaciente,
    IEliminarPacienteUseCase eliminarPaciente,
    IActualizarFotoCedulaUseCase actualizarFotoCedula,
    IObtenerUrlFotoCedulaUseCase obtenerUrlFotoCedula,
    IImportarPacientesUseCase importarPacientes) : ControllerBase
{
    [HttpGet]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Doctor, RolUsuario.Fondos, RolUsuario.Lemy)]
    public async Task<ActionResult<PacientesPaginadosDto>> ListarAsync(
        [FromQuery] int pagina, [FromQuery] int tamanoPagina, [FromQuery] string? busqueda,
        [FromQuery] EstadoPaciente? estado, CancellationToken cancellationToken) =>
        Ok(await listarPacientes.EjecutarAsync(
            new ListarPacientesRequest(pagina, tamanoPagina, busqueda, estado), cancellationToken));

    [HttpGet("{id:guid}")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Doctor, RolUsuario.Fondos, RolUsuario.Lemy)]
    public async Task<ActionResult<PacienteDto>> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await obtenerPacientePorId.EjecutarAsync(id, cancellationToken));

    [HttpPost]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Doctor, RolUsuario.Fondos, RolUsuario.Lemy)]
    public async Task<ActionResult<PacienteDto>> CrearAsync(
        CrearPacienteRequest request, CancellationToken cancellationToken)
    {
        var paciente = await crearPaciente.EjecutarAsync(request, cancellationToken);
        return Created("api/pacientes", paciente);
    }

    [HttpPatch]
    [RequiereRol(RolUsuario.Lemy)]
    public async Task<ActionResult<PacienteDto>> EditarAsync(
        EditarPacienteRequest request, CancellationToken cancellationToken) =>
        Ok(await editarPaciente.EjecutarAsync(request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy, RolUsuario.Doctor)]
    public async Task<IActionResult> EliminarAsync(Guid id, CancellationToken cancellationToken)
    {
        await eliminarPaciente.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/foto-cedula")]
    [RequiereRol(RolUsuario.Lemy)]
    public async Task<ActionResult<PacienteDto>> ActualizarFotoCedulaAsync(
        Guid id, IFormFile archivo, CancellationToken cancellationToken)
    {
        if (!archivo.EsFotoPerfilValida(out var error))
        {
            return BadRequest(new { titulo = "Solicitud inválida", detalle = error });
        }

        await using var contenido = archivo.OpenReadStream();
        var paciente = await actualizarFotoCedula.EjecutarAsync(
            new ActualizarFotoCedulaRequest(id, contenido, archivo.ContentType), cancellationToken);

        return Ok(paciente);
    }

    [HttpGet("{id:guid}/foto-cedula")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<UrlFotoCedulaDto>> ObtenerUrlFotoCedulaAsync(
        Guid id, CancellationToken cancellationToken) =>
        Ok(await obtenerUrlFotoCedula.EjecutarAsync(id, cancellationToken));

    [HttpPost("importar")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<ImportarPacientesResultDto>> ImportarAsync(
        IFormFile archivo, CancellationToken cancellationToken)
    {
        if (!archivo.EsExcelValido(out var error))
        {
            return BadRequest(new { titulo = "Solicitud inválida", detalle = error });
        }

        await using var contenido = archivo.OpenReadStream();
        return Ok(await importarPacientes.EjecutarAsync(contenido, cancellationToken));
    }
}
