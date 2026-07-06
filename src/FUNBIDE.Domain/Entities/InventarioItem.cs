using FUNBIDE.Domain.Common;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Exceptions;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Ítem de inventario de farmacia. Encapsula la única invariante crítica del módulo:
/// el stock nunca puede quedar negativo.
/// </summary>
public sealed class InventarioItem : Entity
{
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int StockActual { get; private set; }
    public CategoriaInventario Categoria { get; private set; }
    public int StockMinimo { get; private set; }

    private InventarioItem() { }

    public InventarioItem(string codigo, string nombre, int stockInicial, CategoriaInventario categoria, int stockMinimo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código del ítem es obligatorio.", nameof(codigo));
        }

        if (stockInicial < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stockInicial), "El stock inicial no puede ser negativo.");
        }

        if (stockMinimo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stockMinimo), "El stock mínimo no puede ser negativo.");
        }

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
        StockActual = stockInicial;
        Categoria = categoria;
        StockMinimo = stockMinimo;
    }

    /// <summary>
    /// Aplica un descargo de inventario. Lanza <see cref="StockInsuficienteException"/>
    /// si la cantidad solicitada supera el stock disponible, evitando que el agregado
    /// quede en un estado inválido antes de llegar a la base de datos.
    /// </summary>
    public void DescargarStock(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad a descargar debe ser mayor que cero.");
        }

        if (cantidad > StockActual)
        {
            throw new StockInsuficienteException(Codigo, StockActual, cantidad);
        }

        StockActual -= cantidad;
    }

    public void Reingresar(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad a reingresar debe ser mayor que cero.");
        }

        StockActual += cantidad;
    }
}
