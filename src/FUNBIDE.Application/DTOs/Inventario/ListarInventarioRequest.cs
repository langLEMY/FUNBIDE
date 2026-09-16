namespace FUNBIDE.Application.DTOs.Inventario;

/// <summary>
/// Todos los campos son opcionales para que `new ListarInventarioRequest()` reproduzca
/// exactamente el comportamiento de antes (traer todo el inventario) — ver
/// ListarInventarioUseCase.
/// </summary>
public sealed record ListarInventarioRequest(string? Busqueda = null, int? Pagina = null, int? TamanoPagina = null);
