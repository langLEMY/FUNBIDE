using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class TurnoCajaRepository(FunbideDbContext dbContext) : ITurnoCajaRepository
{
    // Tracked (no AsNoTracking): CerrarTurnoCajaUseCase muta el turno abierto devuelto
    // por este método y lo guarda en la misma unidad de trabajo.
    public Task<TurnoCaja?> ObtenerAbiertoAsync(CancellationToken cancellationToken) =>
        dbContext.TurnosCaja.FirstOrDefaultAsync(t => t.Estado == EstadoTurnoCaja.Abierto, cancellationToken);

    public Task<TurnoCaja?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.TurnosCaja.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AgregarAsync(TurnoCaja turno, CancellationToken cancellationToken) =>
        await dbContext.TurnosCaja.AddAsync(turno, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
