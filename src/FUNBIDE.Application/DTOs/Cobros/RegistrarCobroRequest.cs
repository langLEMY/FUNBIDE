using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Cobros;

public sealed record RegistrarCobroRequest(
    Guid PacienteId,
    Guid? CitaId,
    string Concepto,
    decimal MontoTotal,
    MetodoPago MetodoPago,
    decimal MontoPagado,
    Guid? SeguroMedicoId,
    string? CodigoAutorizacion);
