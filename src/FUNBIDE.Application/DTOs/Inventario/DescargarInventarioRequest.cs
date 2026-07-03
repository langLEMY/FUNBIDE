namespace FUNBIDE.Application.DTOs.Inventario;

public sealed record DescargarInventarioRequest(Guid InventarioItemId, int Cantidad, string? Referencia);
