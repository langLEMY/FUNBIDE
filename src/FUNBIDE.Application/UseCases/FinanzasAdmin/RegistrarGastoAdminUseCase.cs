using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.FinanzasAdmin;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.FinanzasAdmin;

public interface IRegistrarGastoAdminUseCase : IUseCase<RegistrarGastoAdminRequest, MovimientoImportanteDto>
{
}

/// <summary>
/// Registra un gasto organizacional (alquiler, nómina, insumos) desde el panel de Admin.
/// A diferencia de <c>RegistrarMovimientoFinancieroUseCase</c> (el de Caja/Fondos), este
/// caso de uso NO exige un turno de caja abierto: son conceptos distintos — Caja
/// reconcilia efectivo físico de un turno, Admin registra gastos de la fundación en
/// cualquier momento. Ambos escriben en la misma tabla (<see cref="MovimientoFinanciero"/>)
/// para que aparezcan juntos en la vista consolidada de movimientos de Admin.
/// </summary>
public sealed class RegistrarGastoAdminUseCase(
    IMovimientoFinancieroRepository movimientoRepository,
    IResumenDiarioRepository resumenDiarioRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : IRegistrarGastoAdminUseCase
{
    public Task<MovimientoImportanteDto> EjecutarAsync(RegistrarGastoAdminRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            var movimiento = new MovimientoFinanciero(
                TipoMovimientoFinanciero.Egreso, request.Monto, request.Concepto, currentUser.UsuarioId);

            await movimientoRepository.RegistrarAsync(movimiento, ct);
            await movimientoRepository.GuardarCambiosAsync(ct);

            var ahoraLocal = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.ZonaHorariaClinica);
            var hoy = DateOnly.FromDateTime(ahoraLocal.DateTime);
            var resumen = await resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(hoy, ct);
            resumen.AcumularMovimiento(movimiento.MontoConSigno);
            await resumenDiarioRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "finanzas-admin.registrar-gasto",
                recurso: $"movimientos-financieros/{movimiento.Id}",
                detalle: new { movimiento.Monto, movimiento.Concepto },
                usuarioId: currentUser.UsuarioId,
                codigoRespuestaHttp: 201,
                cancellationToken: ct);

            return new MovimientoImportanteDto(
                movimiento.Id, movimiento.RegistradoEn, "Manual", movimiento.Tipo, movimiento.Concepto, movimiento.Monto, null);
        }, cancellationToken);
    }
}
