using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Comprobante inmutable de un movimiento de dinero (ingreso o egreso) registrado por
/// FONDOS. Espejo del patrón de <see cref="MovimientoInventario"/>: append-only, sin
/// edición ni borrado, para que el historial de caja sea auditable.
/// </summary>
public sealed class MovimientoFinanciero : AppendOnlyEntity
{
    public TipoMovimientoFinanciero Tipo { get; private set; }
    public decimal Monto { get; private set; }
    public string Concepto { get; private set; } = string.Empty;
    public Guid UsuarioId { get; private set; }
    public Guid? CitaId { get; private set; }

    /// <summary>
    /// Turno de caja en el que se registró el movimiento (ver <see cref="TurnoCaja"/>).
    /// Nullable para no romper filas históricas de antes de que existiera el concepto de
    /// turno; todo movimiento nuevo lo trae siempre, exigido por
    /// <c>RegistrarMovimientoFinancieroUseCase</c>.
    /// </summary>
    public Guid? TurnoCajaId { get; private set; }

    private MovimientoFinanciero() { }

    public MovimientoFinanciero(
        TipoMovimientoFinanciero tipo, decimal monto, string concepto, Guid usuarioId, Guid? citaId = null, Guid? turnoCajaId = null)
    {
        if (monto <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monto), "El monto debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(concepto))
        {
            throw new ArgumentException("El concepto del movimiento es obligatorio.", nameof(concepto));
        }

        Tipo = tipo;
        Monto = monto;
        Concepto = concepto.Trim();
        UsuarioId = usuarioId;
        CitaId = citaId;
        TurnoCajaId = turnoCajaId;
    }

    /// <summary>Ingreso suma, egreso resta: usado para acumular el neto del día en <see cref="ResumenDiario"/>.</summary>
    public decimal MontoConSigno => Tipo == TipoMovimientoFinanciero.Ingreso ? Monto : -Monto;
}
