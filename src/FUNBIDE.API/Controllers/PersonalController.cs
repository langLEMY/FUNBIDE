using FUNBIDE.API.Authorization;
using FUNBIDE.API.Extensions;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.UseCases.Personal;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// Administración del personal: crear/eliminar perfiles, cambiar roles y contraseñas,
/// y subir la foto de referencia de cualquier miembro del personal (p. ej. el ícono de
/// un doctor en el panel). Exclusivo del rol LEMY.
/// </summary>
[ApiController]
[Route("api/personal")]
[Authorize]
[RequiereRol(RolUsuario.Lemy)]
public sealed class PersonalController(
    IListarPersonalUseCase listarPersonal,
    ICrearUsuarioUseCase crearUsuario,
    IEditarUsuarioUseCase editarUsuario,
    ICambiarRolUsuarioUseCase cambiarRol,
    ICambiarContrasenaUsuarioUseCase cambiarContrasena,
    IActualizarFotoPerfilUseCase actualizarFoto,
    IEliminarUsuarioUseCase eliminarUsuario,
    IEliminarUsuarioPermanentementeUseCase eliminarUsuarioPermanentemente,
    IReactivarUsuarioUseCase reactivarUsuario) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> ListarAsync(CancellationToken cancellationToken) =>
        Ok(await listarPersonal.EjecutarAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> CrearAsync(CrearUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = await crearUsuario.EjecutarAsync(request, cancellationToken);
        return Created("api/personal", usuario);
    }

    [HttpPatch("datos")]
    public async Task<ActionResult<UsuarioDto>> EditarAsync(
        EditarUsuarioRequest request, CancellationToken cancellationToken) =>
        Ok(await editarUsuario.EjecutarAsync(request, cancellationToken));

    [HttpPatch("rol")]
    public async Task<ActionResult<UsuarioDto>> CambiarRolAsync(
        CambiarRolRequest request, CancellationToken cancellationToken) =>
        Ok(await cambiarRol.EjecutarAsync(request, cancellationToken));

    [HttpPatch("contrasena")]
    public async Task<ActionResult<UsuarioDto>> CambiarContrasenaAsync(
        CambiarContrasenaRequest request, CancellationToken cancellationToken) =>
        Ok(await cambiarContrasena.EjecutarAsync(request, cancellationToken));

    [HttpPost("{id:guid}/foto")]
    public async Task<ActionResult<UsuarioDto>> ActualizarFotoAsync(
        Guid id, IFormFile archivo, CancellationToken cancellationToken)
    {
        if (!archivo.EsFotoPerfilValida(out var error))
        {
            return BadRequest(new { titulo = "Solicitud inválida", detalle = error });
        }

        await using var contenido = archivo.OpenReadStream();
        var usuario = await actualizarFoto.EjecutarAsync(
            new ActualizarFotoPerfilRequest(id, contenido, archivo.ContentType), cancellationToken);

        return Ok(usuario);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> EliminarAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await eliminarUsuario.EjecutarAsync(id, cancellationToken));

    /// <summary>Irreversible: borra al usuario de Supabase Auth, no solo le revoca el acceso.</summary>
    [HttpDelete("{id:guid}/permanente")]
    public async Task<ActionResult<UsuarioDto>> EliminarPermanentementeAsync(
        Guid id, CancellationToken cancellationToken) =>
        Ok(await eliminarUsuarioPermanentemente.EjecutarAsync(id, cancellationToken));

    [HttpPatch("{id:guid}/reactivar")]
    public async Task<ActionResult<UsuarioDto>> ReactivarAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await reactivarUsuario.EjecutarAsync(id, cancellationToken));
}
