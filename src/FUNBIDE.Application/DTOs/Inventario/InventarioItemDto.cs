using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Inventario;

public sealed record InventarioItemDto(
    Guid Id, string Codigo, string Nombre, int StockActual, CategoriaInventario Categoria, int StockMinimo);
