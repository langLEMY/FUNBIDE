namespace FUNBIDE.Application.DTOs.Inventario;

public sealed record EditarInventarioItemRequest(
    Guid InventarioItemId, string Nombre, int StockActual, int StockMinimo);
