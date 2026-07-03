using FUNBIDE.API.Extensions;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.UseCases.Personal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Autoservicio de perfil disponible para cualquier rol autenticado: alimenta el menú
/// que cuelga de la foto de perfil (esquina superior derecha) con "Ver perfil" y
/// "Cambiar foto de perfil". La tercera opción del menú, "Cerrar sesión", no tiene
/// contraparte aquí: con JWT sin estado de Supabase, cerrar sesión es responsabilidad
/// exclusiva del frontend (descarta el token); no hay nada que revocar en el backend.
/// </summary>
[ApiController]
[Route("api/mi-perfil")]
[Authorize]
public sealed class MiPerfilController(
    IVerPerfilPropioUseCase verPerfil,
    IActualizarFotoPerfilPropiaUseCase actualizarFotoPropia) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UsuarioDto>> VerAsync(CancellationToken cancellationToken) =>
        Ok(await verPerfil.EjecutarAsync(cancellationToken));

    [HttpPost("foto")]
    public async Task<ActionResult<UsuarioDto>> ActualizarFotoAsync(
        IFormFile archivo, CancellationToken cancellationToken)
    {
        if (!archivo.EsFotoPerfilValida(out var error))
        {
            return BadRequest(new { titulo = "Solicitud inválida", detalle = error });
        }

        await using var contenido = archivo.OpenReadStream();
        var usuario = await actualizarFotoPropia.EjecutarAsync(
            new ActualizarFotoPerfilPropiaRequest(contenido, archivo.ContentType), cancellationToken);

        return Ok(usuario);
    }
}
