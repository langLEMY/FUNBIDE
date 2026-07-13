using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Caja;

public interface IAbrirTurnoCajaUseCase : IUseCase<AbrirTurnoCajaRequest, TurnoCajaDto>
{
}

/// <summary>
/// Abre un turno de caja con el monto inicial en efectivo contado. Mientras esté abierto,
/// los cobros y egresos quedan ligados a este turno (ver <see cref="Cobro.TurnoCajaId"/> y
/// <see cref="MovimientoFinanciero.TurnoCajaId"/>) para que el cierre pueda reconciliarlos.
/// </summary>
public sealed class AbrirTurnoCajaUseCase(
    ITurnoCajaRepository turnoCajaRepository,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : IAbrirTurnoCajaUseCase
{
    public async Task<TurnoCajaDto> EjecutarAsync(AbrirTurnoCajaRequest request, CancellationToken cancellationToken)
    {
        var turnoAbierto = await turnoCajaRepository.ObtenerAbiertoAsync(cancellationToken);
        if (turnoAbierto is not null)
        {
            throw new InvalidOperationException("Ya hay un turno de caja abierto. Ciérralo antes de abrir uno nuevo.");
        }

        var turno = new TurnoCaja(currentUser.UsuarioId, request.MontoInicial, dateTimeProvider.UtcNow);

        await turnoCajaRepository.AgregarAsync(turno, cancellationToken);
        await turnoCajaRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "caja.abrir-turno",
            recurso: $"turnos-caja/{turno.Id}",
            detalle: new { turno.MontoInicial },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 201,
            cancellationToken: cancellationToken);

        return new TurnoCajaDto(
            turno.Id, turno.UsuarioAperturaId, turno.MontoInicial, turno.AbiertoEn, turno.Estado,
            turno.UsuarioCierreId, turno.MontoFinalContado, turno.MontoEsperado, turno.Diferencia, turno.Notas, turno.CerradoEn);
    }
}
