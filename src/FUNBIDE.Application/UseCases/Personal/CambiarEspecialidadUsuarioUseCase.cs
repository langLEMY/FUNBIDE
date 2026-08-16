using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface ICambiarEspecialidadUsuarioUseCase : IUseCase<CambiarEspecialidadRequest, UsuarioDto>
{
}

/// <summary>
/// Asigna la especialidad médica de un usuario con rol Doctor. Solo aplica a perfiles
/// Doctor — <see cref="Domain.Entities.Usuario.AsignarEspecialidad"/> rechaza el cambio
/// para cualquier otro rol.
/// </summary>
public sealed class CambiarEspecialidadUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICambiarEspecialidadUsuarioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(CambiarEspecialidadRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), request.UsuarioId);

        if (usuario.Rol != RolUsuario.Doctor)
        {
            throw new OperacionNoPermitidaException("Solo un usuario con rol Doctor puede tener una especialidad.");
        }

        usuario.AsignarEspecialidad(request.Especialidad);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.cambiar-especialidad",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { request.Especialidad },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.NombreUsuario, usuario.Rol, usuario.Activo, usuario.FotoPerfilUrl, usuario.Especialidad);
    }
}
