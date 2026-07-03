using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Comprobante inmutable de un movimiento de inventario, generado como consecuencia
/// de <see cref="InventarioItem.DescargarStock"/> o <see cref="InventarioItem.Reingresar"/>.
/// </summary>
public sealed class MovimientoInventario : AppendOnlyEntity
{
    public Guid InventarioItemId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public TipoMovimientoInventario Tipo { get; private set; }
    public int Cantidad { get; private set; }
    public int StockResultante { get; private set; }
    public string? Referencia { get; private set; }

    private MovimientoInventario() { }

    public MovimientoInventario(
        Guid inventarioItemId,
        Guid usuarioId,
        TipoMovimientoInventario tipo,
        int cantidad,
        int stockResultante,
        string? referencia)
    {
        InventarioItemId = inventarioItemId;
        UsuarioId = usuarioId;
        Tipo = tipo;
        Cantidad = cantidad;
        StockResultante = stockResultante;
        Referencia = referencia;
    }
}
