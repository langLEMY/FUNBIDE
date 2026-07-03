using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Finanzas;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Finanzas;

public interface IRegistrarMovimientoFinancieroUseCase : IUseCase<RegistrarMovimientoFinancieroRequest, MovimientoFinancieroDto>
{
}

/// <summary>
/// Registra un ingreso o egreso de forma atómica y acumula su monto (con signo) sobre
/// el <see cref="ResumenDiario"/> del día en curso, dentro de la misma transacción que
/// bloquea la fila del resumen — mismo patrón que <c>DescargarInventarioUseCase</c>.
/// </summary>
public sealed class RegistrarMovimientoFinancieroUseCase(
    IMovimientoFinancieroRepository movimientoRepository,
    IResumenDiarioRepository resumenDiarioRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : IRegistrarMovimientoFinancieroUseCase
{
    public Task<MovimientoFinancieroDto> EjecutarAsync(
        RegistrarMovimientoFinancieroRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            var movimiento = new MovimientoFinanciero(
                request.Tipo, request.Monto, request.Concepto, currentUser.UsuarioId, request.CitaId);

            await movimientoRepository.RegistrarAsync(movimiento, ct);
            await movimientoRepository.GuardarCambiosAsync(ct);

            var hoy = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
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
                movimiento.CitaId, movimiento.RegistradoEn);
        }, cancellationToken);
    }
}
