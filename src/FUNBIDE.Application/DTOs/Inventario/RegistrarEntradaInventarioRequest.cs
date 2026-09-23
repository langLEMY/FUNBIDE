namespace FUNBIDE.Application.DTOs.Inventario;

public sealed record RegistrarEntradaInventarioRequest(Guid InventarioItemId, int Cantidad, string? Referencia);
