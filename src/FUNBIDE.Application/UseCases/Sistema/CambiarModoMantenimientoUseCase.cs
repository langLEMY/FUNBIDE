using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Sistema;

public interface ICambiarModoMantenimientoUseCase : IUseCase<CambiarModoMantenimientoRequest, ModoMantenimientoDto>
{
}

/// <summary>
/// Prende o apaga el modo mantenimiento. Quien realmente bloquea el tráfico es
/// <c>MantenimientoMiddleware</c> en la API, que lee esta misma configuración en cada
/// request; este caso de uso solo la persiste. Solo LEMY puede invocarlo (ver
/// <c>[RequiereRol]</c> en <c>MantenimientoController</c>).
/// </summary>
public sealed class CambiarModoMantenimientoUseCase(
    IConfiguracionSistemaRepository configuracionRepository,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : ICambiarModoMantenimientoUseCase
{
    public async Task<ModoMantenimientoDto> EjecutarAsync(
        CambiarModoMantenimientoRequest request, CancellationToken cancellationToken)
    {
        var ahora = dateTimeProvider.UtcNow;
        var configuracion = await configuracionRepository.ObtenerAsync(cancellationToken);

        if (configuracion is null)
        {
            configuracion = new ConfiguracionSistema(currentUser.UsuarioId, ahora);
            await configuracionRepository.AgregarAsync(configuracion, cancellationToken);
        }

        configuracion.CambiarModoMantenimiento(request.Activo, request.Mensaje, currentUser.UsuarioId, ahora);
        await configuracionRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "sistema.modo-mantenimiento",
            recurso: "sistema/modo-mantenimiento",
            detalle: new { configuracion.ModoMantenimientoActivo, configuracion.ModoMantenimientoMensaje },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new ModoMantenimientoDto(configuracion.ModoMantenimientoActivo, configuracion.ModoMantenimientoMensaje);
    }
}
