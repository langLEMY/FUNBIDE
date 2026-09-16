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
/// (no en la cita) porque <see cref="Domain.Entities.Cita"/> no guarda cuándo se completó.
/// Un cobro se atribuye a un doctor de dos formas, en este orden: primero por
/// <see cref="Domain.Entities.Cobro.DoctorId"/> (asignado directo, típico de un pago
/// particular sin cita) y, si ese campo es null, por el doctor de la cita asociada — antes
/// solo se miraba la cita, así que todo cobro particular con doctor asignado directo (sin
/// CitaId) quedaba fuera del resumen de ese doctor.
/// </summary>
public sealed class ObtenerResumenPorDoctorUseCase(
    ICobroRepository cobroRepository,
    ICitaRepository citaRepository) : IObtenerResumenPorDoctorUseCase
{
    public async Task<ResumenPorDoctorDto> EjecutarAsync(ResumenPorDoctorRequest request, CancellationToken cancellationToken)
    {
        var (desde, hasta) = RangoFechasHelper.RecortarASpanMaximo(request.Desde, request.Hasta);
        var cobros = await cobroRepository.ObtenerPorRangoAsync(desde, hasta, cancellationToken);

        var cobrosSinDoctorDirecto = cobros.Where(c => c.DoctorId is null && c.CitaId.HasValue).ToList();
        var citaIds = cobrosSinDoctorDirecto.Select(c => c.CitaId!.Value).Distinct().ToList();
        var doctorPorCita = citaIds.Count > 0
            ? await citaRepository.ObtenerDoctorIdsPorCitaIdsAsync(citaIds, cancellationToken)
            : new Dictionary<Guid, Guid>();

        Guid? ResolverDoctorId(Domain.Entities.Cobro c) =>
            c.DoctorId ?? (c.CitaId.HasValue && doctorPorCita.TryGetValue(c.CitaId.Value, out var doctorId) ? doctorId : null);

        var cobrosDelDoctor = cobros.Where(c => ResolverDoctorId(c) == request.DoctorId).ToList();

        var pacientesAtendidos = cobrosDelDoctor.Select(c => c.PacienteId).Distinct().Count();
        var dineroGenerado = cobrosDelDoctor.Sum(c => c.MontoTotal);

        return new ResumenPorDoctorDto(pacientesAtendidos, dineroGenerado);
    }
}
