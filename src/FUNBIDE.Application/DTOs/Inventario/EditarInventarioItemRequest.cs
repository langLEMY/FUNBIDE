namespace FUNBIDE.Application.DTOs.Inventario;

// Ya no trae StockActual: el stock ya no se corrige escribiendo un número absoluto en
// "Editar" — todo cambio de stock pasa por un movimiento explícito (RegistrarEntradaInventarioUseCase
// o DescargarInventarioUseCase), para que quede en el historial quién movió qué y cuándo.
public sealed record EditarInventarioItemRequest(Guid InventarioItemId, string Nombre, int StockMinimo);
