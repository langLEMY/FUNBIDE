namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Raised when a pharmacy stock discharge would drive an inventory item's stock below zero.
/// </summary>
public sealed class StockInsuficienteException : DomainException
{
    public StockInsuficienteException(string codigoItem, int stockActual, int cantidadSolicitada)
        : base($"Stock insuficiente para el item '{codigoItem}'. Disponible: {stockActual}, solicitado: {cantidadSolicitada}.")
    {
        CodigoItem = codigoItem;
        StockActual = stockActual;
        CantidadSolicitada = cantidadSolicitada;
    }

    public string CodigoItem { get; }
    public int StockActual { get; }
    public int CantidadSolicitada { get; }
}
