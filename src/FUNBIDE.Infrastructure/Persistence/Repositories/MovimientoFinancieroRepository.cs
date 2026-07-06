using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class MovimientoFinancieroRepository(FunbideDbContext dbContext) : IMovimientoFinancieroRepository
{
    public async Task<IReadOnlyList<MovimientoFinanciero>> ObtenerTodosAsync(CancellationToken cancellationToken) =>
        await dbContext.MovimientosFinancieros
            .AsNoTracking()
            .OrderByDescending(m => m.RegistradoEn)
            .ToListAsync(cancellationToken);

    public async Task RegistrarAsync(MovimientoFinanciero movimiento, CancellationToken cancellationToken) =>
        await dbContext.MovimientosFinancieros.AddAsync(movimiento, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
