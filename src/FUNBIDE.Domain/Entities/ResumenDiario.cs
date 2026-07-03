using FUNBIDE.Domain.Common;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Acumulador mutable de un día calendario: pacientes atendidos y dinero movido neto.
/// A diferencia de las entidades append-only del sistema, esta sí se actualiza in-place
/// a medida que ocurren eventos (una cita se completa, se registra un movimiento
/// financiero) — es un agregado derivado, no un comprobante. Alimenta las tarjetas
/// "hoy" y el gráfico "Vista del mes" del dashboard de ADMIN.
/// </summary>
public sealed class ResumenDiario : Entity
{
    public DateOnly Fecha { get; private set; }
    public int PacientesAtendidos { get; private set; }
    public decimal DineroMovido { get; private set; }

    private ResumenDiario() { }

    public ResumenDiario(DateOnly fecha)
    {
        Fecha = fecha;
    }

    public void RegistrarPacienteAtendido() => PacientesAtendidos++;

    public void AcumularMovimiento(decimal montoConSigno) => DineroMovido += montoConSigno;
}
