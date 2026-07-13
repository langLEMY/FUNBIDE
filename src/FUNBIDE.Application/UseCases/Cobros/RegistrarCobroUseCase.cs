using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Cobros;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Cobros;

public interface IRegistrarCobroUseCase : IUseCase<RegistrarCobroRequest, CobroDto>
{
}

/// <summary>
/// Registra el cobro a un paciente. Si trae seguro médico, el % de cobertura se lee
/// siempre del catálogo (<see cref="SeguroMedico"/>) en este momento y se congela en el
/// comprobante — nunca se confía en un porcentaje que mande el cliente. Acumula
/// <see cref="Cobro.MontoPagado"/> (lo efectivamente cobrado, no lo cubierto por
/// seguro) en el <see cref="ResumenDiario"/> del día, igual que
/// <c>RegistrarMovimientoFinancieroUseCase</c> — sin esto, el neto que ve el
/// dashboard de Admin no incluiría ningún ingreso cobrado en Caja.
/// </summary>
public sealed class RegistrarCobroUseCase(
    ICobroRepository cobroRepository,
    ITurnoCajaRepository turnoCajaRepository,
    ISeguroMedicoRepository seguroMedicoRepository,
    IPacienteRepository pacienteRepository,
    IResumenDiarioRepository resumenDiarioRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuditoriaLogService auditoriaLogService) : IRegistrarCobroUseCase
{
    public Task<CobroDto> EjecutarAsync(RegistrarCobroRequest request, CancellationToken cancellationToken)
    {
        return unitOfWork.EjecutarEnTransaccionAsync(async ct =>
        {
            var turno = await turnoCajaRepository.ObtenerAbiertoAsync(ct)
                ?? throw new InvalidOperationException("No hay una caja abierta. Abre la caja antes de registrar un cobro.");

            var paciente = await pacienteRepository.ObtenerPorIdAsync(request.PacienteId, ct)
                ?? throw new RecursoNoEncontradoException(nameof(Paciente), request.PacienteId);

            if (request.CitaId.HasValue)
            {
                var yaCobrada = await cobroRepository.ExisteCobroParaCitaAsync(request.CitaId.Value, ct);
                if (yaCobrada)
                {
                    throw new CitaYaCobradaException(request.CitaId.Value);
                }
            }

            SeguroMedico? seguro = null;
            if (request.SeguroMedicoId.HasValue)
            {
                seguro = await seguroMedicoRepository.ObtenerPorIdAsync(request.SeguroMedicoId.Value, ct)
                    ?? throw new RecursoNoEncontradoException(nameof(SeguroMedico), request.SeguroMedicoId.Value);

                if (!seguro.Activo)
                {
                    throw new InvalidOperationException($"La aseguradora '{seguro.Nombre}' está desactivada y no puede usarse en nuevos cobros.");
                }
            }

            var cobro = new Cobro(
                request.PacienteId,
                request.CitaId,
                turno.Id,
                currentUser.UsuarioId,
                request.Concepto,
                request.MontoTotal,
                request.MetodoPago,
                request.MontoPagado,
                seguro?.Id,
                seguro?.PorcentajeCobertura,
                request.CodigoAutorizacion);

            await cobroRepository.AgregarAsync(cobro, ct);
            await cobroRepository.GuardarCambiosAsync(ct);

            var hoy = DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);
            var resumen = await resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(hoy, ct);
            resumen.AcumularMovimiento(cobro.MontoPagado);
            await resumenDiarioRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "cobros.registrar",
                recurso: $"cobros/{cobro.Id}",
                detalle: new { cobro.PacienteId, cobro.MontoTotal, cobro.MetodoPago, cobro.MontoPagado, cobro.SeguroMedicoId },
                usuarioId: currentUser.UsuarioId,
                codigoRespuestaHttp: 201,
                cancellationToken: ct);

            return new CobroDto(
                cobro.Id, cobro.PacienteId, paciente.NombreCompleto, cobro.CitaId, cobro.TurnoCajaId, cobro.Concepto,
                cobro.MontoTotal, cobro.SeguroMedicoId, seguro?.Nombre, cobro.PorcentajeCobertura, cobro.MontoCobertura,
                cobro.CodigoAutorizacion, cobro.MetodoPago, cobro.MontoACargoPaciente, cobro.MontoPagado, cobro.MontoPendiente,
                cobro.UsuarioId, cobro.RegistradoEn);
        }, cancellationToken);
    }
}
