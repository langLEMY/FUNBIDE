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
    ITarifarioProcedimientoRepository tarifarioRepository,
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
            // Bloqueo de fila (no un simple SELECT): sin esto, un cobro podía colarse
            // justo mientras CerrarTurnoCajaUseCase está calculando el efectivo esperado
            // del arqueo, quedando fuera de esa cuenta aunque el turno siguiera "Abierto"
            // en el instante en que este cobro lo leyó.
            var turno = await turnoCajaRepository.ObtenerAbiertoConBloqueoAsync(ct)
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

            // El monto total y el copago de un procedimiento del tarifario (hoy solo
            // SENASA) son fijos y se releen del catálogo, igual que el % de cobertura de
            // las demás aseguradoras: lo que mande el cliente en MontoTotal se descarta.
            TarifarioProcedimiento? tarifario = null;
            if (request.TarifarioProcedimientoId.HasValue)
            {
                tarifario = await tarifarioRepository.ObtenerPorIdAsync(request.TarifarioProcedimientoId.Value, ct)
                    ?? throw new RecursoNoEncontradoException(nameof(TarifarioProcedimiento), request.TarifarioProcedimientoId.Value);

                if (!tarifario.Activo)
                {
                    throw new InvalidOperationException($"El procedimiento '{tarifario.Procedimiento}' del tarifario está desactivado.");
                }

                if (seguro is null || tarifario.SeguroMedicoId != seguro.Id)
                {
                    throw new InvalidOperationException("El procedimiento del tarifario no corresponde a la aseguradora seleccionada.");
                }
            }

            var montoTotalReal = tarifario?.MontoTotal ?? request.MontoTotal;
            var pagos = (request.Pagos ?? []).Select(p => new PagoRecibido(p.Metodo, p.Monto)).ToList();

            var cobro = new Cobro(
                request.PacienteId,
                request.CitaId,
                turno.Id,
                currentUser.UsuarioId,
                request.Concepto,
                montoTotalReal,
                pagos,
                seguro?.Id,
                tarifario is null ? seguro?.PorcentajeCobertura : null,
                request.CodigoAutorizacion,
                tarifario?.Id,
                tarifario?.MontoSeguro);

            await cobroRepository.AgregarAsync(cobro, ct);
            await cobroRepository.GuardarCambiosAsync(ct);

            var ahoraLocal = TimeZoneInfo.ConvertTime(dateTimeProvider.UtcNow, dateTimeProvider.ZonaHorariaClinica);
            var hoy = DateOnly.FromDateTime(ahoraLocal.DateTime);
            var resumen = await resumenDiarioRepository.ObtenerOCrearConBloqueoAsync(hoy, ct);
            resumen.AcumularMovimiento(cobro.MontoPagado);
            foreach (var pago in cobro.Pagos)
            {
                // Desglose por método (ver ResumenDiario): lo que alimenta el panel de
                // Admin "cómo entra el dinero" — sin esto, esa vista no tendría de dónde
                // sacar cuánto entró en efectivo vs. tarjeta vs. transferencia por día.
                resumen.AcumularPago(pago.Metodo, pago.Monto);
            }
            await resumenDiarioRepository.GuardarCambiosAsync(ct);

            await auditoriaLogService.RegistrarEventoAsync(
                accion: "cobros.registrar",
                recurso: $"cobros/{cobro.Id}",
                detalle: new { cobro.PacienteId, cobro.MontoTotal, Pagos = cobro.Pagos.Select(p => new { p.Metodo, p.Monto }), cobro.SeguroMedicoId },
                usuarioId: currentUser.UsuarioId,
                codigoRespuestaHttp: 201,
                cancellationToken: ct);

            return new CobroDto(
                cobro.Id, cobro.PacienteId, paciente.NombreCompleto, cobro.CitaId, cobro.TurnoCajaId, cobro.Concepto,
                cobro.MontoTotal, cobro.SeguroMedicoId, seguro?.Nombre, cobro.PorcentajeCobertura, cobro.MontoCobertura,
                cobro.CodigoAutorizacion, cobro.Pagos.Select(p => new PagoDto(p.Metodo, p.Monto)).ToList(),
                cobro.MontoACargoPaciente, cobro.MontoPagado, cobro.MontoPendiente,
                cobro.UsuarioId, cobro.RegistradoEn, cobro.TarifarioProcedimientoId);
        }, cancellationToken);
    }
}
