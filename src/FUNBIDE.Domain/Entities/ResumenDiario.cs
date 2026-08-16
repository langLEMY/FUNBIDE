using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Acumulador mutable de un día calendario: pacientes atendidos y dinero movido neto,
/// desglosado por método de pago. A diferencia de las entidades append-only del sistema,
/// esta sí se actualiza in-place a medida que ocurren eventos (una cita se completa, se
/// registra un cobro) — es un agregado derivado, no un comprobante. Alimenta las
/// tarjetas "hoy" y el gráfico "Vista del mes" del dashboard de ADMIN, incluido el
/// panel "cómo entra el dinero" (<see cref="DineroEfectivo"/>/<see cref="DineroTarjeta"/>/
/// <see cref="DineroTransferencia"/>).
/// </summary>
public sealed class ResumenDiario : Entity
{
    public DateOnly Fecha { get; private set; }
    public int PacientesAtendidos { get; private set; }
    public decimal DineroMovido { get; private set; }
    public decimal DineroEfectivo { get; private set; }
    public decimal DineroTarjeta { get; private set; }
    public decimal DineroTransferencia { get; private set; }

    private ResumenDiario() { }

    public ResumenDiario(DateOnly fecha)
    {
        Fecha = fecha;
    }

    public void RegistrarPacienteAtendido() => PacientesAtendidos++;

    public void AcumularMovimiento(decimal montoConSigno) => DineroMovido += montoConSigno;

    /// <summary>
    /// Suma un pago concreto (una línea de <see cref="Cobro.Pagos"/>) al desglose por
    /// método. Independiente de <see cref="AcumularMovimiento"/> (que sigue siendo el
    /// neto general, con signo, usado también por movimientos manuales de Finanzas sin
    /// método asociado) — un cobro llama a ambos.
    /// </summary>
    public void AcumularPago(MetodoPago metodo, decimal monto)
    {
        switch (metodo)
        {
            case MetodoPago.Efectivo:
                DineroEfectivo += monto;
                break;
            case MetodoPago.Tarjeta:
                DineroTarjeta += monto;
                break;
            case MetodoPago.Transferencia:
                DineroTransferencia += monto;
                break;
        }
    }
}
