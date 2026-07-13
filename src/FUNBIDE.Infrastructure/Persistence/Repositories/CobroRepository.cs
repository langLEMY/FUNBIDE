using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class CobroRepository(FunbideDbContext dbContext) : ICobroRepository
{
    public async Task<IReadOnlyList<Cobro>> ObtenerPorTurnoAsync(Guid turnoCajaId, CancellationToken cancellationToken) =>
        await dbContext.Cobros
            .AsNoTracking()
            .Where(c => c.TurnoCajaId == turnoCajaId)
            .OrderByDescending(c => c.RegistradoEn)
            .ToListAsync(cancellationToken);

    // Se suma la fórmula de MontoPendiente sobre las columnas reales (MontoTotal,
    // MontoCobertura, MontoPagado) en vez de la propiedad calculada del dominio: EF Core
    // no puede traducir esta última a SQL.
    public Task<decimal> ObtenerDeudaTotalPorPacienteAsync(Guid pacienteId, CancellationToken cancellationToken) =>
        dbContext.Cobros
            .AsNoTracking()
            .Where(c => c.PacienteId == pacienteId)
            .SumAsync(c => c.MontoTotal - (c.MontoCobertura ?? 0) - c.MontoPagado, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, decimal>> ObtenerDeudaTotalPorPacientesAsync(
        IReadOnlyCollection<Guid> pacienteIds, CancellationToken cancellationToken)
    {
        if (pacienteIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        return await dbContext.Cobros
            .AsNoTracking()
            .Where(c => pacienteIds.Contains(c.PacienteId))
            .GroupBy(c => c.PacienteId)
            .Select(g => new { PacienteId = g.Key, Deuda = g.Sum(c => c.MontoTotal - (c.MontoCobertura ?? 0) - c.MontoPagado) })
            .ToDictionaryAsync(x => x.PacienteId, x => x.Deuda, cancellationToken);
    }

    public Task<int> ContarPacientesConDeudaAsync(CancellationToken cancellationToken) =>
        dbContext.Cobros
            .AsNoTracking()
            .Where(c => c.MontoTotal - (c.MontoCobertura ?? 0) - c.MontoPagado > 0)
            .Select(c => c.PacienteId)
            .Distinct()
            .CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Cobro>> ObtenerPorRangoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, CancellationToken cancellationToken) =>
        await dbContext.Cobros
            .AsNoTracking()
            .Where(c => c.RegistradoEn >= desde && c.RegistradoEn < hasta)
            .OrderByDescending(c => c.RegistradoEn)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteCobroParaCitaAsync(Guid citaId, CancellationToken cancellationToken) =>
        dbContext.Cobros.AnyAsync(c => c.CitaId == citaId, cancellationToken);

    public async Task AgregarAsync(Cobro cobro, CancellationToken cancellationToken) =>
        await dbContext.Cobros.AddAsync(cobro, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
