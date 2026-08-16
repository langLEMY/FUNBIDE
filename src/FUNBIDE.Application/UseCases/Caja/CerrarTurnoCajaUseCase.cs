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
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : ICerrarTurnoCajaUseCase
{
    public Task<TurnoCajaDto> EjecutarAsync(CerrarTurnoCajaRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            // Bloqueo de fila (ver ObtenerAbiertoConBloqueoAsync): serializa este cierre
            // contra cualquier RegistrarCobroUseCase concurrente sobre el mismo turno, para
            // que ningún cobro quede fuera del cálculo de "efectivo esperado" de abajo por
            // una ventana de lectura no atómica entre ambos casos de uso.
            var turno = await turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(ct)
                ?? throw new InvalidOperationException("No hay un turno de caja abierto para cerrar.");

            // Misma fórmula que ObtenerResumenCajaUseCase.EfectivoEnCaja: lo que debería
            // haber en la caja física en efectivo. Se calcula acá (no se reutiliza el
            // resumen) porque acá se congela en el turno junto con la diferencia contra lo
            // contado.
            var cobros = await cobroRepository.ObtenerPorTurnoAsync(turno.Id, ct);
            var movimientos = await movimientoRepository.ObtenerPorTurnoAsync(turno.Id, ct);
            // Por línea de pago (Cobro.Pagos), no por cobro entero: un cobro puede traer
            // varias líneas con métodos distintos (ver "dividir el pago").
            var totalEfectivo = cobros.SelectMany(c => c.Pagos).Where(p => p.Metodo == MetodoPago.Efectivo).Sum(p => p.Monto);
            var salidasAutorizadas = movimientos.Where(m => m.Tipo == TipoMovimientoFinanciero.Egreso).Sum(m => m.Monto);
            var ingresosManuales = movimientos.Where(m => m.Tipo == TipoMovimientoFinanciero.Ingreso).Sum(m => m.Monto);
            var montoEsperado = turno.MontoInicial + totalEfectivo + ingresosManuales - salidasAutorizadas;

            turno.Cerrar(currentUser.UsuarioId, request.MontoFinalContado, montoEsperado, request.Notas, dateTimeProvider.UtcNow);
            await turnoCajaRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "caja.cerrar-turno",
                recurso: $"turnos-caja/{turno.Id}",
                detalle: new { turno.MontoInicial, turno.MontoFinalContado, turno.MontoEsperado, turno.Diferencia, turno.Notas },
                usuarioId: currentUser.UsuarioId,
                codigoRespuestaHttp: 200,
                cancellationToken: ct);

            return new TurnoCajaDto(
                turno.Id, turno.UsuarioAperturaId, turno.MontoInicial, turno.AbiertoEn, turno.Estado,
                turno.UsuarioCierreId, turno.MontoFinalContado, turno.MontoEsperado, turno.Diferencia, turno.Notas, turno.CerradoEn);
        }, cancellationToken);
    }
}
