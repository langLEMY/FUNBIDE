using System.Data;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence;

/// <summary>
/// Envuelve la operación en una transacción PostgreSQL real. Se usa ReadCommitted
/// junto con el bloqueo de fila explícito (SELECT ... FOR UPDATE) de
/// <see cref="Repositories.InventarioRepository.ObtenerConBloqueoAsync"/> en lugar de
/// Serializable: el bloqueo de fila ya serializa el acceso al ítem concreto sin pagar
/// el costo de reintentos por fallos de serialización de todo el nivel Serializable.
/// </summary>
public sealed class UnitOfWork(FunbideDbContext dbContext) : IUnitOfWork
{
    public async Task<TResultado> EjecutarEnTransaccionAsync<TResultado>(
        Func<CancellationToken, Task<TResultado>> operacion, CancellationToken cancellationToken)
    {
        await using var transaccion = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            var resultado = await operacion(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return resultado;
        }
        catch
        {
            await transaccion.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
