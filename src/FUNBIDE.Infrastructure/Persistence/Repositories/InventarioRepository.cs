using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class InventarioRepository(FunbideDbContext dbContext) : IInventarioRepository
{
    /// <summary>
    /// SELECT ... FOR UPDATE: adquiere un bloqueo exclusivo de fila en PostgreSQL.
    /// Cualquier otra transacción que intente descargar el mismo ítem espera hasta
    /// que esta transacción haga commit o rollback, evitando condiciones de carrera
    /// sobre <see cref="InventarioItem.StockActual"/>.
    /// </summary>
    public async Task<InventarioItem?> ObtenerConBloqueoAsync(Guid inventarioItemId, CancellationToken cancellationToken)
    {
        var resultados = await dbContext.InventarioItems
            .FromSqlInterpolated(
                $"SELECT *, xmin FROM funbide.inventario_items WHERE \"Id\" = {inventarioItemId} FOR UPDATE")
            .ToListAsync(cancellationToken);

        return resultados.SingleOrDefault();
    }

    public Task<InventarioItem?> ObtenerPorIdAsync(Guid inventarioItemId, CancellationToken cancellationToken) =>
        dbContext.InventarioItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == inventarioItemId, cancellationToken);

    public async Task<IReadOnlyList<InventarioItem>> ObtenerTodosAsync(CancellationToken cancellationToken) =>
        await dbContext.InventarioItems
            .AsNoTracking()
            .OrderBy(i => i.Nombre)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken) =>
        dbContext.InventarioItems.AsNoTracking().AnyAsync(i => i.Codigo == codigo, cancellationToken);

    public async Task AgregarAsync(InventarioItem item, CancellationToken cancellationToken) =>
        await dbContext.InventarioItems.AddAsync(item, cancellationToken);

    public async Task RegistrarMovimientoAsync(MovimientoInventario movimiento, CancellationToken cancellationToken) =>
        await dbContext.MovimientosInventario.AddAsync(movimiento, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
