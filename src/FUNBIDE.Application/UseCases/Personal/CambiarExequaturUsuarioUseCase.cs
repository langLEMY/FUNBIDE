using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Personal;

public interface ICambiarExequaturUsuarioUseCase : IUseCase<CambiarExequaturRequest, UsuarioDto>
{
}

/// <summary>
/// Asigna el número de exequátur (colegiatura médica) de un usuario con rol Doctor —
/// se imprime en cada receta/orden médica para que sea válida ante una farmacia o
/// laboratorio. Solo aplica a perfiles Doctor, igual que <see cref="CambiarEspecialidadUsuarioUseCase"/>.
/// </summary>
public sealed class CambiarExequaturUsuarioUseCase(
    IUsuarioRepository usuarioRepository,
    ICurrentUserService currentUser,
    IAuditoriaLogService auditoriaLogService) : ICambiarExequaturUsuarioUseCase
{
    public async Task<UsuarioDto> EjecutarAsync(CambiarExequaturRequest request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.Usuario), request.UsuarioId);

        if (usuario.Rol != RolUsuario.Doctor)
        {
            throw new OperacionNoPermitidaException("Solo un usuario con rol Doctor puede tener un número de exequátur.");
        }

        usuario.AsignarExequatur(request.Exequatur);
        await usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "personal.cambiar-exequatur",
            recurso: $"usuarios/{usuario.Id}",
            detalle: new { request.Exequatur },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new UsuarioDto(
            usuario.Id, usuario.NombreCompleto, usuario.Correo, usuario.NombreUsuario, usuario.Rol,
            usuario.Activo, usuario.FotoPerfilUrl, usuario.Especialidad, Exequatur: usuario.Exequatur);
    }
}
