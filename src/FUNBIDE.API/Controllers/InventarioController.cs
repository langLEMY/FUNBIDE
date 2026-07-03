using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Application.UseCases.Inventario;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Endpoint transaccional de farmacia. El descargo de stock corre dentro de una
/// transacción de base de datos con bloqueo de fila (ver <c>DescargarInventarioUseCase</c>
/// e <c>InventarioRepository.ObtenerConBloqueoAsync</c>) para garantizar atomicidad
/// incluso bajo descargos concurrentes del mismo ítem.
/// </summary>
[ApiController]
[Route("api/inventario")]
[Authorize]
[RequiereRol(RolUsuario.Farmacia)]
public sealed class InventarioController(IDescargarInventarioUseCase descargarInventario) : ControllerBase
{
    [HttpPost("descargo")]
    public async Task<ActionResult<MovimientoInventarioDto>> DescargarAsync(
        DescargarInventarioRequest request, CancellationToken cancellationToken) =>
        Ok(await descargarInventario.EjecutarAsync(request, cancellationToken));
}
