using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Citas;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Citas;

public interface IObtenerCitasPorEstadoUseCase : IUseCase<EstadoCita, IReadOnlyList<CitaDto>>
{
}

/// <summary>
/// Lista las citas del doctor autenticado filtradas por estado (pendientes,
/// programadas o completadas), tal como exige el panel de DOCTOR.
/// </summary>
public sealed class ObtenerCitasPorEstadoUseCase(
    ICitaRepository citaRepository,
    ICurrentUserService currentUser) : IObtenerCitasPorEstadoUseCase
{
    public async Task<IReadOnlyList<CitaDto>> EjecutarAsync(EstadoCita estado, CancellationToken cancellationToken)
    {
        var citas = await citaRepository.ObtenerPorDoctorYEstadoAsync(currentUser.UsuarioId, estado, cancellationToken);

        return citas
            .Select(c => new CitaDto(
                c.Id,
                c.PacienteId,
                c.DoctorId,
                c.Motivo,
                c.Estado,
                c.Intervalo?.Inicio,
                c.Intervalo?.Fin,
                c.NotasCierre))
            .ToList();
    }
}
