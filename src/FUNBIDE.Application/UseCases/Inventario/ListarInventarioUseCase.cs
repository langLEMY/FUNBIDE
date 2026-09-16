using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Inventario;

public interface IListarInventarioUseCase : IUseCase<ListarInventarioRequest, IReadOnlyList<InventarioItemDto>>
{
}

public sealed class ListarInventarioUseCase(IInventarioRepository inventarioRepository) : IListarInventarioUseCase
{
    private const int TamanoSinPaginacionExplicita = 10_000;
    private const int TamanoPaginaPorDefecto = 50;
    private const int TamanoPaginaMaximo = 100;
    private const int LongitudMinimaBusqueda = 2;

    public async Task<IReadOnlyList<InventarioItemDto>> EjecutarAsync(
        ListarInventarioRequest request, CancellationToken cancellationToken)
    {
        int pagina;
        int tamanoPagina;
        if (request.Pagina is null && request.TamanoPagina is null)
        {
            // Comportamiento de hoy en InventarioPage.tsx: trae todo de una sola vez.
            pagina = 1;
            tamanoPagina = TamanoSinPaginacionExplicita;
        }
        else
        {
            pagina = Math.Clamp(request.Pagina ?? 1, 1, 1_000_000);
            tamanoPagina = request.TamanoPagina is null or < 1
                ? TamanoPaginaPorDefecto
                : Math.Min(request.TamanoPagina.Value, TamanoPaginaMaximo);
        }

        var busqueda = request.Busqueda?.Trim().Length >= LongitudMinimaBusqueda ? request.Busqueda : null;

        var (items, _) = await inventarioRepository.ObtenerPaginadoAsync(busqueda, pagina, tamanoPagina, cancellationToken);

        return items
            .Select(i => new InventarioItemDto(i.Id, i.Codigo, i.Nombre, i.StockActual, i.Categoria, i.StockMinimo))
            .ToList();
    }
}
