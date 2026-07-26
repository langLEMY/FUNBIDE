using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IListarPacientesDelDoctorUseCase : IUseCase<IReadOnlyList<PacienteDelDoctorDto>>
{
}

/// <summary>
/// Pacientes del doctor autenticado, para la tarjeta "Pacientes totales" de su Dashboard.
/// El dominio no tiene un vínculo directo Paciente-Doctor: se deriva de qué pacientes
/// tuvieron alguna vez una cita (cualquier estado) con este doctor.
/// </summary>
public sealed class ListarPacientesDelDoctorUseCase(
    ICitaRepository citaRepository,
    IPacienteRepository pacienteRepository,
    ICurrentUserService currentUser) : IListarPacientesDelDoctorUseCase
{
    public async Task<IReadOnlyList<PacienteDelDoctorDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var pacienteIds = await citaRepository.ObtenerPacienteIdsDistintosPorDoctorAsync(currentUser.UsuarioId, cancellationToken);
        var nombres = await pacienteRepository.ObtenerNombresPorIdsAsync(pacienteIds, cancellationToken);

        return pacienteIds
            .Select(id => new PacienteDelDoctorDto(id, nombres.GetValueOrDefault(id, "Paciente desconocido")))
            .OrderBy(p => p.NombreCompleto)
            .ToList();
    }
}
