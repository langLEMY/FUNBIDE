using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Cobros;

public sealed record CobroDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNombre,
    Guid? CitaId,
    Guid TurnoCajaId,
    string Concepto,
    decimal MontoTotal,
    Guid? SeguroMedicoId,
    string? SeguroMedicoNombre,
    decimal? PorcentajeCobertura,
    decimal? MontoCobertura,
    string? CodigoAutorizacion,
    MetodoPago MetodoPago,
    decimal MontoACargoPaciente,
    decimal MontoPagado,
    decimal MontoPendiente,
    Guid UsuarioId,
    DateTimeOffset RegistradoEn);
