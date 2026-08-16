using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Sistema;

public interface IReiniciarServiciosUseCase : IUseCase<SesionesRevocadasDto>
{
}

/// <summary>
/// "Reiniciar servicios": marca todas las sesiones (JWT) emitidas hasta ahora como
/// inválidas. Quien realmente rechaza los tokens viejos es
/// <c>SesionRevocadaMiddleware</c> en la API, que lee esta misma configuración en cada
/// request; este caso de uso solo persiste la marca de tiempo. Solo LEMY puede
/// invocarlo (ver <c>[RequiereRol]</c> en el controller), y afecta también a su propia
/// sesión — no hay excepción para quien lo ejecuta.
/// </summary>
public sealed class ReiniciarServiciosUseCase(
    IConfiguracionSistemaRepository configuracionRepository,
    IConfiguracionSistemaCache configuracionCache,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : IReiniciarServiciosUseCase
{
    public async Task<SesionesRevocadasDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var ahora = dateTimeProvider.UtcNow;
        var configuracion = await configuracionRepository.ObtenerAsync(cancellationToken);

        if (configuracion is null)
        {
            configuracion = new ConfiguracionSistema(currentUser.UsuarioId, ahora);
            await configuracionRepository.AgregarAsync(configuracion, cancellationToken);
        }

        configuracion.RevocarSesiones(currentUser.UsuarioId, ahora);
        await configuracionRepository.GuardarCambiosAsync(cancellationToken);
        configuracionCache.Invalidar();

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "sistema.reiniciar-servicios",
            recurso: "sistema/reiniciar-servicios",
            detalle: new { SesionesRevocadasEn = ahora },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new SesionesRevocadasDto(ahora);
    }
}
