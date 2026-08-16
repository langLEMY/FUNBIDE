namespace FUNBIDE.Application.DTOs.Dashboard;

public sealed record ResumenDiarioDto(
    DateOnly Fecha,
    int PacientesAtendidos,
    decimal DineroMovido,
    /// <summary>Desglose de cómo entró el dinero ese día (ver Cobro.Pagos) — para el panel "Cómo entra el dinero" de Admin.</summary>
    decimal DineroEfectivo,
    decimal DineroTarjeta,
    decimal DineroTransferencia);
