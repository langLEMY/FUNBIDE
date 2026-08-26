using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Dashboard;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Dashboard;

public interface IObtenerPacientesPorDoctorUseCase : IUseCase<IReadOnlyList<PacientesPorDoctorDto>>
{
}

/// <summary>
/// Cuántas citas completó cada doctor activo y a cuántos pacientes distintos, para la
/// tarjeta "Pacientes por doctor" del Dashboard de Admin. Incluye doctores en cero (sin
/// ninguna cita completada todavía) en vez de omitirlos, para que la lista siempre
/// refleje la plantilla completa de doctores activos.
/// </summary>
public sealed class ObtenerPacientesPorDoctorUseCase(
    ICitaRepository citaRepository,
    IUsuarioRepository usuarioRepository) : IObtenerPacientesPorDoctorUseCase
{
    public async Task<IReadOnlyList<PacientesPorDoctorDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var doctores = await usuarioRepository.ObtenerActivosPorRolAsync(RolUsuario.Doctor, cancellationToken);
        var citasCompletadas = await citaRepository.ObtenerDoctorYPacientePorCompletadasAsync(cancellationToken);

        var porDoctor = citasCompletadas
            .GroupBy(c => c.DoctorId)
            .ToDictionary(
                g => g.Key,
                g => (CitasCompletadas: g.Count(), PacientesDistintos: g.Select(c => c.PacienteId).Distinct().Count()));

        return doctores
            .Select(d =>
            {
                var (citas, pacientes) = porDoctor.GetValueOrDefault(d.SupabaseUserId);
                return new PacientesPorDoctorDto(d.SupabaseUserId, d.NombreCompleto, d.Especialidad?.ToString(), citas, pacientes);
            })
            .OrderByDescending(dto => dto.PacientesDistintos)
            .ThenBy(dto => dto.NombreCompleto)
            .ToList();
    }
}
