using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Finanzas;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Finanzas;

public interface IRegistrarMovimientoFinancieroUseCase : IUseCase<RegistrarMovimientoFinancieroRequest, MovimientoFinancieroDto>
{
}

/// <summary>
/// Registra un ingreso o egreso de forma atómica y acumula su monto (con signo) sobre
/// el <see cref="ResumenDiario"/> del día en curso, dentro de la misma transacción que
/// bloquea la fila del resumen — mismo patrón que <c>DescargarInventarioUseCase</c>.
/// Exige un turno de caja abierto (<see cref="TurnoCaja"/>) y lo adjunta al movimiento,
/// igual que <c>RegistrarCobroUseCase</c>, para que el arqueo de cierre pueda sumarlo.
/// </summary>
public sealed class RegistrarMovimientoFinancieroUseCase(
    IMovimientoFinancieroRepository movimientoRepository,
    IResumenDiarioRepository resumenDiarioRepository,
    ITurnoCajaRepository turnoCajaRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService,
    INotificadorTiempoRealService notificadorTiempoReal) : IRegistrarMovimientoFinancieroUseCase
{
    public async Task<MovimientoFinancieroDto> EjecutarAsync(
        RegistrarMovimientoFinancieroRequest request, CancellationToken cancellationToken)
    {
        var resultado = await unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            // Igual que RegistrarCobroUseCase: si no hay turno abierto, este mismo
            // movimiento lo abre con el fondo fijo (TurnoCaja.FondoFijo).
            var turno = await turnoCajaRepository.ObtenerAbiertoConBloqueoOAbrirAsync(
                currentUser.UsuarioId, dateTimeProvider.UtcNow, ct);

            var movimiento = new MovimientoFinanciero(
                request.Tipo, request.Monto, request.Concepto, currentUser.UsuarioId, request.CitaId, turno.Id);

            await movimientoRepository.RegistrarAsync(movimiento, ct);
            await movimientoRepository.GuardarCambiosAsync(ct);

            var ahoraLocal = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.ZonaHorariaClinica);
            var hoy = DateOnly.FromDateTime(ahoraLocal.DateTime);
            var resumen = await resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(hoy, ct);
            resumen.AcumularMovimiento(movimiento.MontoConSigno);
            await resumenDiarioRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "finanzas.registrar-movimiento",
                recurso: $"movimientos-financieros/{movimiento.Id}",
                detalle: new { movimiento.Tipo, movimiento.Monto, movimiento.Concepto },
                usuarioId: currentUser.UsuarioId,
                codigoRespuestaHttp: 201,
                cancellationToken: ct);

            return new MovimientoFinancieroDto(
                movimiento.Id, movimiento.Tipo, movimiento.Monto, movimiento.Concepto,
                movimiento.CitaId, movimiento.TurnoCajaId, movimiento.RegistradoEn);
        }, cancellationToken);

        try
        {
            await notificadorTiempoReal.NotificarEventoCajaAsync(
                new EventoCajaDto(
                    "movimiento", resultado.Concepto, resultado.Monto,
                    EsIngreso: resultado.Tipo == TipoMovimientoFinanciero.Ingreso, resultado.RegistradoEn),
                cancellationToken);
        }
        catch (Exception)
        {
            // Best-effort — ver RegistrarCobroUseCase.
        }

        return resultado;
    }
}
