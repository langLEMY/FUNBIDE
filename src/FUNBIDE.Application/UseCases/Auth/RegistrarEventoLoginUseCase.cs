using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Auth;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Auth;

public interface IRegistrarEventoLoginUseCase : IUseCase<RegistrarEventoLoginComando, bool>
{
}

/// <summary>
/// Registra un intento de inicio de sesión (éxito o fallo) en la auditoría. El login
/// en sí ocurre directamente contra Supabase Auth desde el navegador, sin pasar por
/// esta API, así que este es el único punto donde el backend se entera de un intento
/// — de ahí que el endpoint que invoca este caso de uso sea anónimo.
/// </summary>
public sealed class RegistrarEventoLoginUseCase(
    IUsuarioRepository usuarioRepository,
    IAuditoriaLogService auditoriaLogService) : IRegistrarEventoLoginUseCase
{
    public async Task<bool> EjecutarAsync(RegistrarEventoLoginComando request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioRepository.ObtenerPorNombreUsuarioAsync(
            request.NombreUsuario.Trim().ToLowerInvariant(), cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: request.Exitoso ? "auth.login.exitoso" : "auth.login.fallido",
            recurso: "auth/login",
            detalle: new { request.NombreUsuario, request.Ip, request.Dispositivo },
            usuarioId: usuario?.Id,
            codigoRespuestaHttp: request.Exitoso ? 200 : 401,
            cancellationToken: cancellationToken);

        return usuario is not null;
    }
}
