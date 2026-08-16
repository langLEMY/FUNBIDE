using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Application.UseCases.Sistema;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Acciones de troubleshooting reservadas a LEMY ("Herramientas de soporte" y "Avanzado"
/// en Mi Perfil): cerrar sesiones, forzar un backup fuera de horario, revisar espacio en
/// disco. Separado de <see cref="MantenimientoController"/> porque ese solo prende/apaga
/// mantenimiento, aunque comparte el mismo gateo de rol.
/// </summary>
[ApiController]
[Route("api/sistema/avanzado")]
[Authorize]
[RequiereRol(RolUsuario.Lemy)]
public sealed class SistemaAvanzadoController(
    IReiniciarServiciosUseCase reiniciarServicios,
    IEjecutarBackupManualUseCase ejecutarBackupManual,
    IVerificarEspacioDiscoUseCase verificarEspacioDisco) : ControllerBase
{
    [HttpPost("reiniciar-servicios")]
    public async Task<ActionResult<SesionesRevocadasDto>> ReiniciarServiciosAsync(CancellationToken cancellationToken) =>
        Ok(await reiniciarServicios.EjecutarAsync(cancellationToken));

    [HttpPost("backup-manual")]
    public async Task<ActionResult<ResultadoBackupManualDto>> EjecutarBackupManualAsync(CancellationToken cancellationToken) =>
        Ok(await ejecutarBackupManual.EjecutarAsync(cancellationToken));

    [HttpGet("espacio-disco")]
    public async Task<ActionResult<EstadoDiscoDto>> VerificarEspacioDiscoAsync(CancellationToken cancellationToken) =>
        Ok(await verificarEspacioDisco.EjecutarAsync(cancellationToken));
}
