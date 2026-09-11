using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.HistorialClinico;

public sealed record EntradaHistorialDto(
    Guid Id,
    Guid PacienteId,
    Guid DoctorId,
    Guid? CitaId,
    TipoEntradaHistorial Tipo,
    string Contenido,
    DateTimeOffset RegistradoEn);
