namespace FUNBIDE.Application.DTOs.Resumen;

public sealed record ResumenPorDoctorRequest(Guid DoctorId, DateTimeOffset Desde, DateTimeOffset Hasta);
