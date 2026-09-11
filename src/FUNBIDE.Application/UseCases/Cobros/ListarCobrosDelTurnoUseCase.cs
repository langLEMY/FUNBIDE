using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Cobros;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Cobros;

public interface IListarCobrosDelTurnoUseCase : IUseCase<IReadOnlyList<CobroDto>>
{
}

/// <summary>Historial de cobros del turno de caja actual, para el panel lateral del dashboard.</summary>
public sealed class ListarCobrosDelTurnoUseCase(
    ICobroRepository cobroRepository,
    ITurnoCajaRepository turnoCajaRepository,
    IPacienteRepository pacienteRepository,
    IUsuarioRepository usuarioRepository,
    ISeguroMedicoRepository seguroMedicoRepository) : IListarCobrosDelTurnoUseCase
{
    public async Task<IReadOnlyList<CobroDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var turno = await turnoCajaRepository.ObtenerAbiertoAsync(cancellationToken);
        if (turno is null)
        {
            return [];
        }

        var cobros = await cobroRepository.ObtenerPorTurnoAsync(turno.Id, cancellationToken);
        if (cobros.Count == 0)
        {
            return [];
        }

        var nombresPacientes = await pacienteRepository.ObtenerNombresPorIdsAsync(
            cobros.Select(c => c.PacienteId).Distinct().ToList(), cancellationToken);
        var nombresSeguros = (await seguroMedicoRepository.ObtenerTodosAsync(incluirInactivos: true, cancellationToken))
            .ToDictionary(s => s.Id, s => s.Nombre);
        var nombresDoctores = await usuarioRepository.ObtenerNombresPorIdsAsync(
            cobros.Where(c => c.DoctorId.HasValue).Select(c => c.DoctorId!.Value).Distinct().ToList(), cancellationToken);

        return cobros
            .Select(c => new CobroDto(
                c.Id,
                c.PacienteId,
                nombresPacientes.GetValueOrDefault(c.PacienteId, "—"),
                c.CitaId,
                c.TurnoCajaId,
                c.Concepto,
                c.MontoTotal,
                c.SeguroMedicoId,
                c.SeguroMedicoId.HasValue ? nombresSeguros.GetValueOrDefault(c.SeguroMedicoId.Value) : null,
                c.PorcentajeCobertura,
                c.MontoCobertura,
                c.CodigoAutorizacion,
                c.Pagos.Select(p => new PagoDto(p.Metodo, p.Monto)).ToList(),
                c.MontoACargoPaciente,
                c.MontoPagado,
                c.MontoPendiente,
                c.UsuarioId,
                c.RegistradoEn,
                c.TarifarioProcedimientoId,
                c.MontoFondo,
                c.DoctorId,
                c.DoctorId.HasValue ? nombresDoctores.GetValueOrDefault(c.DoctorId.Value) : null))
            .ToList();
    }
}
