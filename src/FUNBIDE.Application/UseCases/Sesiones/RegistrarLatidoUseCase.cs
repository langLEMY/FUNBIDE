using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sesiones;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Sesiones;

public interface IRegistrarLatidoUseCase : IUseCase<RegistrarLatidoRequest, LatidoDto>
{
}

/// <summary>
/// El frontend llama esto cada cierto tiempo mientras la app está abierta y autenticada
/// (ver hook de latido en AuthContext). No es una acción de negocio auditable — es
/// telemetría de presencia, así que a propósito no pasa por <c>IAuditoriaLogService</c>
/// (inundaría la bitácora con un evento por minuto por dispositivo conectado).
/// </summary>
public sealed class RegistrarLatidoUseCase(
    ISesionActivaRepository sesionRepository,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider) : IRegistrarLatidoUseCase
{
    public async Task<LatidoDto> EjecutarAsync(RegistrarLatidoRequest request, CancellationToken cancellationToken)
    {
        var ahora = dateTimeProvider.UtcNow;
        var sesion = await sesionRepository.ObtenerPorUsuarioYSessionIdAsync(
            currentUser.UsuarioId, request.SessionId, cancellationToken);

        if (sesion is null)
        {
            sesion = new SesionActiva(currentUser.UsuarioId, request.SessionId, ahora);
            await sesionRepository.AgregarAsync(sesion, cancellationToken);
        }
        else
        {
            sesion.RegistrarLatido(ahora);
        }

        await sesionRepository.GuardarCambiosAsync(cancellationToken);

        return new LatidoDto(ahora);
    }
}
