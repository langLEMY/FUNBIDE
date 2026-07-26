using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Donaciones;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Donaciones;

public interface IRegistrarDonacionUseCase : IUseCase<RegistrarDonacionRequest, DonacionDto>
{
}

/// <summary>
/// Registra una donación externa. Además de guardar los datos propios del donante, crea
/// en la misma transacción un <see cref="MovimientoFinanciero"/> (Ingreso) por el mismo
/// monto y lo acumula en <see cref="ResumenDiario"/> — mismo patrón que
/// <c>RegistrarGastoAdminUseCase</c> — para que la donación aparezca junto al resto de
/// los ingresos en la vista consolidada de Finanzas sin duplicar la lógica de reportes.
/// </summary>
public sealed class RegistrarDonacionUseCase(
    IDonacionRepository donacionRepository,
    IMovimientoFinancieroRepository movimientoRepository,
    IResumenDiarioRepository resumenDiarioRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : IRegistrarDonacionUseCase
{
    public Task<DonacionDto> EjecutarAsync(RegistrarDonacionRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            var donacion = new Donacion(
                request.DonanteNombre, request.DonanteContacto, request.Monto, request.Concepto, currentUser.UsuarioId);

            await donacionRepository.AgregarAsync(donacion, ct);
            await donacionRepository.GuardarCambiosAsync(ct);

            var concepto = $"Donación - {donacion.DonanteNombre}";
            var movimiento = new MovimientoFinanciero(TipoMovimientoFinanciero.Ingreso, donacion.Monto, concepto, currentUser.UsuarioId);
            await movimientoRepository.RegistrarAsync(movimiento, ct);
            await movimientoRepository.GuardarCambiosAsync(ct);

            var hoy = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
            var resumen = await resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(hoy, ct);
            resumen.AcumularMovimiento(movimiento.MontoConSigno);
            await resumenDiarioRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "donaciones.registrar",
                recurso: $"donaciones/{donacion.Id}",
                detalle: new { donacion.DonanteNombre, donacion.Monto, donacion.Concepto },
                usuarioId: currentUser.UsuarioId,
                codigoRespuestaHttp: 201,
                cancellationToken: ct);

            return new DonacionDto(
                donacion.Id, donacion.DonanteNombre, donacion.DonanteContacto, donacion.Monto, donacion.Concepto, donacion.RegistradoEn);
        }, cancellationToken);
    }
}
