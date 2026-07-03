namespace FUNBIDE.Application.DTOs.Inventario;

public sealed record MovimientoInventarioDto(
    Guid Id,
    Guid InventarioItemId,
    string CodigoItem,
    int Cantidad,
    int StockResultante,
    DateTimeOffset RegistradoEn);
