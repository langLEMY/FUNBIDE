using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sesiones;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Sesiones;

public interface IContarSesionesActivasUseCase : IUseCase<SesionesActivasDto>
{
}

/// <summary>
/// Para la tarjeta "Sesiones activas" del Dashboard de Admin: cuántos dispositivos
/// distintos mandaron un latido en los últimos <see cref="VentanaActiva"/> — bastante más
/// que el intervalo de latido del frontend (1 minuto) para tolerar algún latido perdido
/// sin marcar a alguien como desconectado de más.
/// </summary>
public sealed class ContarSesionesActivasUseCase(
    ISesionActivaRepository sesionRepository,
    IDateTimeProvider dateTimeProvider) : IContarSesionesActivasUseCase
{
    private static readonly TimeSpan VentanaActiva = TimeSpan.FromMinutes(3);

    public async Task<SesionesActivasDto> EjecutarAsync(CancellationToken cancellationToken)
    {
        var desde = dateTimeProvider.UtcNow - VentanaActiva;
        var cantidad = await sesionRepository.ContarActivasDesdeAsync(desde, cancellationToken);
        return new SesionesActivasDto(cantidad);
    }
}
