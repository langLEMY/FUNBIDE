using FUNBIDE.API.Authorization;
using FUNBIDE.API.Extensions;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Application.UseCases.Inventario;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Base de datos de inventario de farmacia. Todos los roles pueden consultar,
/// agregar y despachar medicamentos/insumos. Cada acción declara su propio
/// <see cref="RequiereRolAttribute"/> en vez de uno a nivel de clase, igual que
/// <c>PacientesController</c>.
/// </summary>
[ApiController]
[Route("api/inventario")]
[Authorize]
public sealed class InventarioController(
    IListarInventarioUseCase listarInventario,
    ICrearInventarioItemUseCase crearInventarioItem,
    IEditarInventarioItemUseCase editarInventarioItem,
    IEliminarInventarioItemUseCase eliminarInventarioItem,
    IDescargarInventarioUseCase descargarInventario,
    IImportarInventarioUseCase importarInventario) : ControllerBase
{
    [HttpGet]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Doctor, RolUsuario.Fondos, RolUsuario.Lemy)]
    public async Task<ActionResult<IReadOnlyList<InventarioItemDto>>> ListarAsync(CancellationToken cancellationToken) =>
        Ok(await listarInventario.EjecutarAsync(cancellationToken));

    [HttpPost]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Doctor, RolUsuario.Fondos, RolUsuario.Lemy)]
    public async Task<ActionResult<InventarioItemDto>> CrearAsync(
        CrearInventarioItemRequest request, CancellationToken cancellationToken)
    {
        var item = await crearInventarioItem.EjecutarAsync(request, cancellationToken);
        return Created("api/inventario", item);
    }

    [HttpPatch]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Doctor, RolUsuario.Fondos, RolUsuario.Lemy)]
    public async Task<ActionResult<InventarioItemDto>> EditarAsync(
        EditarInventarioItemRequest request, CancellationToken cancellationToken) =>
        Ok(await editarInventarioItem.EjecutarAsync(request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Doctor, RolUsuario.Fondos, RolUsuario.Lemy)]
    public async Task<IActionResult> EliminarAsync(Guid id, CancellationToken cancellationToken)
    {
        await eliminarInventarioItem.EjecutarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("descargo")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Doctor, RolUsuario.Fondos, RolUsuario.Lemy)]
    public async Task<ActionResult<MovimientoInventarioDto>> DescargarAsync(
        DescargarInventarioRequest request, CancellationToken cancellationToken) =>
        Ok(await descargarInventario.EjecutarAsync(request, cancellationToken));

    [HttpPost("importar")]
    [RequiereRol(RolUsuario.Admin, RolUsuario.Lemy)]
    public async Task<ActionResult<ImportarInventarioResultDto>> ImportarAsync(
        IFormFile archivo, CancellationToken cancellationToken)
    {
        if (!archivo.EsExcelValido(out var error))
        {
            return BadRequest(new { titulo = "Solicitud inválida", detalle = error });
        }

        await using var contenido = archivo.OpenReadStream();
        return Ok(await importarInventario.EjecutarAsync(contenido, cancellationToken));
    }
}
