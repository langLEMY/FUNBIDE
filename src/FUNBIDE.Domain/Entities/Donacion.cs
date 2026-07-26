using FUNBIDE.Domain.Common;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Comprobante inmutable de una donación externa recibida por la fundación: append-only,
/// igual que <see cref="Cobro"/> y <see cref="MovimientoFinanciero"/>. Al registrarse
/// también se crea un <see cref="MovimientoFinanciero"/> (Ingreso) por el mismo monto,
/// para que la donación aparezca junto al resto de los ingresos en la vista consolidada
/// de Finanzas — esta entidad solo guarda los datos propios del donante.
/// </summary>
public sealed class Donacion : AppendOnlyEntity
{
    public string DonanteNombre { get; private set; } = string.Empty;
    public string? DonanteContacto { get; private set; }
    public decimal Monto { get; private set; }
    public string Concepto { get; private set; } = string.Empty;
    public Guid UsuarioId { get; private set; }

    private Donacion() { }

    public Donacion(string donanteNombre, string? donanteContacto, decimal monto, string concepto, Guid usuarioId)
    {
        if (string.IsNullOrWhiteSpace(donanteNombre))
        {
            throw new ArgumentException("El nombre del donante es obligatorio.", nameof(donanteNombre));
        }

        if (monto <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(monto), "El monto de la donación debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(concepto))
        {
            throw new ArgumentException("El concepto de la donación es obligatorio.", nameof(concepto));
        }

        DonanteNombre = donanteNombre.Trim();
        DonanteContacto = string.IsNullOrWhiteSpace(donanteContacto) ? null : donanteContacto.Trim();
        Monto = monto;
        Concepto = concepto.Trim();
        UsuarioId = usuarioId;
    }
}
