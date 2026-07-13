using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Caja;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Caja;

public interface ICerrarTurnoCajaUseCase : IUseCase<CerrarTurnoCajaRequest, TurnoCajaDto>
{
}

/// <summary>Cierra el turno de caja abierto con el arqueo final. Deshabilita nuevos cobros/egresos hasta reabrir.</summary>
public sealed class CerrarTurnoCajaUseCase(
    ITurnoCajaRepository turnoCajaRepository,
    ICobroRepository cobroRepository,
    IMovimientoFinancieroRepository movimientoRepository,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : ICerrarTurnoCajaUseCase
{
    public async Task<TurnoCajaDto> EjecutarAsync(CerrarTurnoCajaRequest request, CancellationToken cancellationToken)
    {
        var turno = await turnoCajaRepository.ObtenerAbiertoAsync(cancellationToken)
            ?? throw new InvalidOperationException("No hay un turno de caja abierto para cerrar.");

        // Misma fórmula que ObtenerResumenCajaUseCase.EfectivoEnCaja: lo que debería haber
        // en la caja física en efectivo. Se calcula acá (no se reutiliza el resumen) porque
        // acá se congela en el turno junto con la diferencia contra lo contado.
        var cobros = await cobroRepository.ObtenerPorTurnoAsync(turno.Id, cancellationToken);
        var movimientos = await movimientoRepository.ObtenerPorTurnoAsync(turno.Id, cancellationToken);
        var totalEfectivo = cobros.Where(c => c.MetodoPago == MetodoPago.Efectivo).Sum(c => c.MontoPagado);
        var salidasAutorizadas = movimientos.Where(m => m.Tipo == TipoMovimientoFinanciero.Egreso).Sum(m => m.Monto);
        var ingresosManuales = movimientos.Where(m => m.Tipo == TipoMovimientoFinanciero.Ingreso).Sum(m => m.Monto);
        var montoEsperado = turno.MontoInicial + totalEfectivo + ingresosManuales - salidasAutorizadas;

        turno.Cerrar(currentUser.UsuarioId, request.MontoFinalContado, montoEsperado, request.Notas, dateTimeProvider.UtcNow);
        await turnoCajaRepository.GuardarCambiosAsync(cancellationToken);

        await auditoriaLogService.RegistrarEventoAsync(
            accion: "caja.cerrar-turno",
            recurso: $"turnos-caja/{turno.Id}",
            detalle: new { turno.MontoInicial, turno.MontoFinalContado, turno.MontoEsperado, turno.Diferencia, turno.Notas },
            usuarioId: currentUser.UsuarioId,
            codigoRespuestaHttp: 200,
            cancellationToken: cancellationToken);

        return new TurnoCajaDto(
            turno.Id, turno.UsuarioAperturaId, turno.MontoInicial, turno.AbiertoEn, turno.Estado,
            turno.UsuarioCierreId, turno.MontoFinalContado, turno.MontoEsperado, turno.Diferencia, turno.Notas, turno.CerradoEn);
    }
}
