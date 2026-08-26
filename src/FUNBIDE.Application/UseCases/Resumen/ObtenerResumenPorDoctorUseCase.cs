using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Resumen;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Resumen;

public interface IObtenerResumenPorDoctorUseCase : IUseCase<ResumenPorDoctorRequest, ResumenPorDoctorDto>
{
}

/// <summary>
/// Pacientes atendidos y dinero generado por un doctor puntual en un rango de fechas, para
/// los widgets filtrables de "Resumen" de Admin. Se ancla en <see cref="Domain.Entities.Cobro.RegistradoEn"/>
/// (no en la cita) porque <see cref="Domain.Entities.Cita"/> no guarda cuándo se completó —
/// un cobro sin <see cref="Domain.Entities.Cobro.CitaId"/> (pago particular sin cita) no se
/// le puede atribuir a ningún doctor y queda fuera de ambos números.
/// </summary>
public sealed class ObtenerResumenPorDoctorUseCase(
    ICobroRepository cobroRepository,
    ICitaRepository citaRepository) : IObtenerResumenPorDoctorUseCase
{
    public async Task<ResumenPorDoctorDto> EjecutarAsync(ResumenPorDoctorRequest request, CancellationToken cancellationToken)
    {
        var cobros = await cobroRepository.ObtenerPorRangoAsync(request.Desde, request.Hasta, cancellationToken);
        var cobrosConCita = cobros.Where(c => c.CitaId.HasValue).ToList();

        if (cobrosConCita.Count == 0)
        {
            return new ResumenPorDoctorDto(0, 0);
        }

        var citaIds = cobrosConCita.Select(c => c.CitaId!.Value).Distinct().ToList();
        var doctorPorCita = await citaRepository.ObtenerDoctorIdsPorCitaIdsAsync(citaIds, cancellationToken);

        var cobrosDelDoctor = cobrosConCita
            .Where(c => doctorPorCita.TryGetValue(c.CitaId!.Value, out var doctorId) && doctorId == request.DoctorId)
            .ToList();

        var pacientesAtendidos = cobrosDelDoctor.Select(c => c.PacienteId).Distinct().Count();
        var dineroGenerado = cobrosDelDoctor.Sum(c => c.MontoTotal);

        return new ResumenPorDoctorDto(pacientesAtendidos, dineroGenerado);
    }
}
