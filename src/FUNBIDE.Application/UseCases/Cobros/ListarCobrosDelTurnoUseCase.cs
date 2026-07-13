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
                c.MetodoPago,
                c.MontoACargoPaciente,
                c.MontoPagado,
                c.MontoPendiente,
                c.UsuarioId,
                c.RegistradoEn))
            .ToList();
    }
}
