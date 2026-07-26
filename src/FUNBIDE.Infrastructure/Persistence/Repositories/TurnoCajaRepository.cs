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

    // Por AbiertoEn (no CerradoEn): un turno abierto en el rango pero sin cerrar todavía
    // también debe verse en la vista de supervisión de Admin.
    public async Task<IReadOnlyList<TurnoCaja>> ObtenerPorRangoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, CancellationToken cancellationToken) =>
        await dbContext.TurnosCaja
            .AsNoTracking()
            .Where(t => t.AbiertoEn >= desde && t.AbiertoEn < hasta)
            .OrderByDescending(t => t.AbiertoEn)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(TurnoCaja turno, CancellationToken cancellationToken) =>
        await dbContext.TurnosCaja.AddAsync(turno, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
