using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class MovimientoFinancieroRepository(FunbideDbContext dbContext) : IMovimientoFinancieroRepository
{
    public async Task RegistrarAsync(MovimientoFinanciero movimiento, CancellationToken cancellationToken) =>
        await dbContext.MovimientosFinancieros.AddAsync(movimiento, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
