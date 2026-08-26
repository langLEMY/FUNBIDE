using FUNBIDE.API.Authorization;
using FUNBIDE.API.Extensions;
using FUNBIDE.Application.DTOs.Servicios;
using FUNBIDE.Application.UseCases.Servicios;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Catálogo de precios privados (pago particular, sin seguro médico): Fondos lo consulta
/// como lista de motivos al agendar/registrar una llegada (Agenda/Recepción) y también
/// como selector de Cobros al cobrar. Admin y Lemy lo administran. Cada acción declara su
/// propio <see cref="RequiereRolAttribute"/>, igual que <c>SegurosMedicosController</c>.
/// </summary>
[ApiController]
[Route("api/servicios")]
[Authorize]
public sealed class ServiciosController(
    IListarServiciosUseCase listarServicios,
    ICrearServicioUseCase crearServicio,
    IEditarServicioUseCase editarServicio,
    IDesactivarServicioUseCase desactivarServicio,
    IReactivarServicioUseCase reactivarServicio,
    IImportarServiciosUseCase importarServicios) : ControllerBase
{
    [HttpGet]
    [RequiereRol(RolUsuario.Fondos, RolUsuario.Admin, RolUsuario.Lemy)]
    [RequierePermiso(ModuloPermiso.Servicios, ModuloPermiso.Cobros, ModuloPermiso.Agenda, ModuloPermiso.Recepcion)]
    public async Task<ActionResult<IReadOnlyList<ServicioDto>>> ListarAsync(
        [FromQuery] bool incluirInactivos, CancellationToken cancellationToken) =>
        Ok(await listarServicios.EjecutarAsync(incluirInactivos, cancellationToken));

    [HttpPost]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    [RequierePermiso(ModuloPermiso.Servicios)]
    public async Task<ActionResult<ServicioDto>> CrearAsync(CrearServicioRequest request, CancellationToken cancellationToken)
    {
        var servicio = await crearServicio.EjecutarAsync(request, cancellationToken);
        return Created("api/servicios", servicio);
    }

    [HttpPatch]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    [RequierePermiso(ModuloPermiso.Servicios)]
    public async Task<ActionResult<ServicioDto>> EditarAsync(EditarServicioRequest request, CancellationToken cancellationToken) =>
        Ok(await editarServicio.EjecutarAsync(request, cancellationToken));

    [HttpPatch("{id:guid}/desactivar")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    [RequierePermiso(ModuloPermiso.Servicios)]
    public async Task<ActionResult<ServicioDto>> DesactivarAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await desactivarServicio.EjecutarAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/reactivar")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    [RequierePermiso(ModuloPermiso.Servicios)]
    public async Task<ActionResult<ServicioDto>> ReactivarAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await reactivarServicio.EjecutarAsync(id, cancellationToken));

    [HttpPost("importar")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    [RequierePermiso(ModuloPermiso.Servicios)]
    public async Task<ActionResult<ImportarServiciosResultDto>> ImportarAsync(
        IFormFile archivo, CancellationToken cancellationToken)
    {
        if (!archivo.EsExcelValido(out var error))
        {
            return BadRequest(new { titulo = "Solicitud inválida", detalle = error });
        }

        await using var contenido = archivo.OpenReadStream();
        return Ok(await importarServicios.EjecutarAsync(contenido, cancellationToken));
    }
}
