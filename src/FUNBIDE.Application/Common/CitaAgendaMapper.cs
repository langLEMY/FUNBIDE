using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.Common;

/// <summary>
/// Arma <see cref="CitaAgendaDto"/> a partir de <see cref="Cita"/>: hace el fetch en lote
/// de nombres de paciente/doctor (para no hacer N consultas) y consulta la deuda de cada
/// paciente involucrado. Compartido por Agenda, Sala de Espera y Pendientes de Cobro para
/// no triplicar la misma lógica de enriquecimiento en cada caso de uso.
/// </summary>
public static class CitaAgendaMapper
{
    public static async Task<IReadOnlyList<CitaAgendaDto>> EnriquecerAsync(
        IReadOnlyList<Cita> citas,
        IPacienteRepository pacienteRepository,
        IUsuarioRepository usuarioRepository,
        ICobroRepository cobroRepository,
        CancellationToken cancellationToken)
    {
        if (citas.Count == 0)
        {
            return [];
        }

        var pacienteIds = citas.Select(c => c.PacienteId).Distinct().ToList();
        var nombresPacientes = await pacienteRepository.ObtenerNombresPorIdsAsync(pacienteIds, cancellationToken);
        var nombresDoctores = await usuarioRepository.ObtenerNombresPorIdsAsync(
            citas.Select(c => c.DoctorId).Distinct().ToList(), cancellationToken);
        var deudasPorPaciente = await cobroRepository.ObtenerDeudaTotalPorPacientesAsync(pacienteIds, cancellationToken);

        var resultado = new List<CitaAgendaDto>(citas.Count);
        foreach (var cita in citas)
        {
            var deuda = deudasPorPaciente.GetValueOrDefault(cita.PacienteId, 0m);
            resultado.Add(new CitaAgendaDto(
                cita.Id,
                cita.PacienteId,
                nombresPacientes.GetValueOrDefault(cita.PacienteId, "—"),
                cita.DoctorId,
                nombresDoctores.GetValueOrDefault(cita.DoctorId, "—"),
                cita.Motivo,
                cita.Estado,
                cita.Intervalo?.Inicio,
                cita.Intervalo?.Fin,
                deuda));
        }

        return resultado;
    }
}
