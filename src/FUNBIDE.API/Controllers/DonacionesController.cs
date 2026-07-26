using FUNBIDE.API.Authorization;
using FUNBIDE.Application.DTOs.Donaciones;
using FUNBIDE.Application.UseCases.Donaciones;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Donaciones externas recibidas por la fundación, separadas de los cobros a pacientes.
/// Append-only (solo alta y listado, nunca edición/borrado de una donación ya registrada).
/// </summary>
[ApiController]
[Route("api/donaciones")]
[Authorize]
[RequiereRol(RolUsuario.Admin)]
[SoloLecturaEInsercion]
public sealed class DonacionesController(
    IListarDonacionesUseCase listarDonaciones,
    IRegistrarDonacionUseCase registrarDonacion) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DonacionDto>>> ListarAsync(
        [FromQuery] DateTimeOffset desde, [FromQuery] DateTimeOffset hasta, CancellationToken cancellationToken) =>
        Ok(await listarDonaciones.EjecutarAsync(new ListarDonacionesRequest(desde, hasta), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<DonacionDto>> RegistrarAsync(
        RegistrarDonacionRequest request, CancellationToken cancellationToken)
    {
        var donacion = await registrarDonacion.EjecutarAsync(request, cancellationToken);
        return Created("api/donaciones", donacion);
    }
}
