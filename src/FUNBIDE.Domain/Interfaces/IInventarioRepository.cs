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

    Task<IReadOnlyList<InventarioItem>> ObtenerTodosAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Con seguimiento de cambios (sin AsNoTracking), para que <c>ImportarInventarioUseCase</c>
    /// pueda reconciliar el archivo contra los ítems existentes (por código) y actualizarlos
    /// en la misma unidad de trabajo. No usar en rutas de solo lectura: para eso está
    /// <see cref="ObtenerTodosAsync"/>.
    /// </summary>
    Task<IReadOnlyList<InventarioItem>> ObtenerTodosParaImportarAsync(CancellationToken cancellationToken);

    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken);

    Task<bool> TieneMovimientosAsync(Guid inventarioItemId, CancellationToken cancellationToken);

    Task AgregarAsync(InventarioItem item, CancellationToken cancellationToken);

    void Eliminar(InventarioItem item);

    Task RegistrarMovimientoAsync(MovimientoInventario movimiento, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
