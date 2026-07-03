using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface IInventarioRepository
{
    /// <summary>
    /// Obtiene el ítem aplicando un bloqueo de fila a nivel de base de datos
    /// (SELECT ... FOR UPDATE) para garantizar aislamiento ante descargos concurrentes.
    /// Debe invocarse dentro de una transacción abierta por <see cref="IUnitOfWork"/>.
    /// </summary>
    Task<InventarioItem?> ObtenerConBloqueoAsync(Guid inventarioItemId, CancellationToken cancellationToken);

    Task<InventarioItem?> ObtenerPorIdAsync(Guid inventarioItemId, CancellationToken cancellationToken);

    Task RegistrarMovimientoAsync(MovimientoInventario movimiento, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
