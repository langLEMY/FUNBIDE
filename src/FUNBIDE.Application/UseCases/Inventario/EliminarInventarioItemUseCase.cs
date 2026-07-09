using FUNBIDE.Application.Common;
using FUNBIDE.Application.Exceptions;
using FUNBIDE.Domain.Exceptions;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Inventario;

public interface IEliminarInventarioItemUseCase : IUseCase<Guid, Guid>
{
}

/// <summary>
/// Bloquea el borrado si el ítem ya tiene movimientos registrados: eliminarlo dejaría
/// esos movimientos huérfanos, violando la trazabilidad del historial de inventario.
/// </summary>
public sealed class EliminarInventarioItemUseCase(
    IInventarioRepository inventarioRepository) : IEliminarInventarioItemUseCase
{
    public async Task<Guid> EjecutarAsync(Guid inventarioItemId, CancellationToken cancellationToken)
    {
        var item = await inventarioRepository.ObtenerPorIdAsync(inventarioItemId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(nameof(Domain.Entities.InventarioItem), inventarioItemId);

        if (await inventarioRepository.TieneMovimientosAsync(inventarioItemId, cancellationToken))
        {
            throw new OperacionNoPermitidaException(
                "No se puede eliminar el ítem: tiene movimientos de inventario registrados.");
        }

        inventarioRepository.Eliminar(item);
        await inventarioRepository.GuardarCambiosAsync(cancellationToken);

        return inventarioItemId;
    }
}
