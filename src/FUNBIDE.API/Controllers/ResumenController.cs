using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Resumen;
using FUNBIDE.Application.UseCases.Resumen;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Widgets filtrables por doctor del "Resumen" de Admin: mismo permiso que
/// <see cref="AuditoriaController"/>, porque alimenta la misma pantalla.
/// </summary>
[ApiController]
[Route("api/resumen")]
[Authorize]
[RequierePermiso(ModuloPermiso.Resumen)]
[SoloLecturaEInsercion]
public sealed class ResumenController(IObtenerResumenPorDoctorUseCase obtenerResumenPorDoctor) : ControllerBase
{
    [HttpGet("por-doctor")]
    public async Task<ActionResult<ResumenPorDoctorDto>> ObtenerPorDoctorAsync(
        [FromQuery] Guid doctorId, [FromQuery] DateTimeOffset desde, [FromQuery] DateTimeOffset hasta,
        CancellationToken cancellationToken) =>
        Ok(await obtenerResumenPorDoctor.EjecutarAsync(new ResumenPorDoctorRequest(doctorId, desde, hasta), cancellationToken));
}
