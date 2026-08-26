namespace FUNBIDE.Application.DTOs.Resumen;

/// <summary>
/// Pacientes distintos y dinero generado por un doctor puntual dentro de un rango de
/// fechas, para los widgets filtrables de "Resumen" de Admin. Se deriva de
/// <see cref="Domain.Entities.Cobro"/> (que sí tiene fecha) en vez de
/// <see cref="Domain.Entities.Cita"/> (que no la tiene) — ver <c>ObtenerResumenPorDoctorUseCase</c>.
/// </summary>
public sealed record ResumenPorDoctorDto(int PacientesAtendidos, decimal DineroGenerado);
