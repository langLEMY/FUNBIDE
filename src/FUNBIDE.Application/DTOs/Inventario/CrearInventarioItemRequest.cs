using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Application.DTOs.Inventario;

public sealed record CrearInventarioItemRequest(
    string Codigo, string Nombre, int StockInicial, CategoriaInventario Categoria, int StockMinimo);
