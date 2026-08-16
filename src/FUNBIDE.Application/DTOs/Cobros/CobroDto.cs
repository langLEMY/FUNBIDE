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
    /// <summary>Desglose de cómo se pagó (ver PagoRecibido) — 0, 1 o varias líneas.</summary>
    IReadOnlyList<PagoDto> Pagos,
    decimal MontoACargoPaciente,
    decimal MontoPagado,
    decimal MontoPendiente,
    Guid UsuarioId,
    DateTimeOffset RegistradoEn,
    Guid? TarifarioProcedimientoId = null);
