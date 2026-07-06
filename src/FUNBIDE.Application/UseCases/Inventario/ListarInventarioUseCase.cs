using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Inventario;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Inventario;

public interface IListarInventarioUseCase : IUseCase<IReadOnlyList<InventarioItemDto>>
{
}

public sealed class ListarInventarioUseCase(IInventarioRepository inventarioRepository) : IListarInventarioUseCase
{
    public async Task<IReadOnlyList<InventarioItemDto>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var items = await inventarioRepository.ObtenerTodosAsync(cancellationToken);

        return items
            .Select(i => new InventarioItemDto(i.Id, i.Codigo, i.Nombre, i.StockActual, i.Categoria, i.StockMinimo))
            .ToList();
    }
}
