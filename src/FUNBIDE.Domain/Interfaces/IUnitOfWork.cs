namespace FUNBIDE.Domain.Interfaces;

/// <summary>
/// Frontera transaccional explícita. La implementación en Infrastructure abre una
/// transacción ReadCommitted y garantiza commit/rollback atómico; el aislamiento
/// efectivo para descargos de inventario lo da el bloqueo de fila explícito
/// (SELECT ... FOR UPDATE) en <see cref="IInventarioRepository.ObtenerConBloqueoAsync"/>.
/// </summary>
public interface IUnitOfWork
{
    Task<TResultado> EjecutarEnTransaccionAsync<TResultado>(
        Func<CancellationToken, Task<TResultado>> operacion, CancellationToken cancellationToken);
}
