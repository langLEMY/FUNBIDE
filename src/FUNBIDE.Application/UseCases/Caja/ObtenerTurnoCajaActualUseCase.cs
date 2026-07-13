using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Caja;

public interface IObtenerTurnoCajaActualUseCase : IUseCase<TurnoCajaDto?>
{
}

/// <summary>Devuelve el turno abierto, o null si la caja está cerrada.</summary>
public sealed class ObtenerTurnoCajaActualUseCase(ITurnoCajaRepository turnoCajaRepository) : IObtenerTurnoCajaActualUseCase
{
    public async Task<TurnoCajaDto?> EjecutarAsync(CancellationToken cancellationToken)
    {
        var turno = await turnoCajaRepository.ObtenerAbiertoAsync(cancellationToken);

        return turno is null
            ? null
            : new TurnoCajaDto(
                turno.Id, turno.UsuarioAperturaId, turno.MontoInicial, turno.AbiertoEn, turno.Estado,
                turno.UsuarioCierreId, turno.MontoFinalContado, turno.MontoEsperado, turno.Diferencia, turno.Notas, turno.CerradoEn);
    }
}
