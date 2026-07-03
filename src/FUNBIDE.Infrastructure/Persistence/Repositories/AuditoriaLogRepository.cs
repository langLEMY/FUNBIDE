using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class AuditoriaLogRepository(FunbideDbContext dbContext) : IAuditoriaLogRepository
{
    public async Task<IReadOnlyList<AuditoriaLog>> ObtenerAsync(
        DateTimeOffset? desde, DateTimeOffset? hasta, string? recurso, CancellationToken cancellationToken)
    {
        var query = dbContext.AuditoriaLogs.AsNoTracking().AsQueryable();

        if (desde is not null)
        {
            query = query.Where(l => l.RegistradoEn >= desde);
        }

        if (hasta is not null)
        {
            query = query.Where(l => l.RegistradoEn <= hasta);
        }

        if (!string.IsNullOrWhiteSpace(recurso))
        {
            query = query.Where(l => l.Recurso == recurso);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task RegistrarAsync(AuditoriaLog log, CancellationToken cancellationToken)
    {
        await dbContext.AuditoriaLogs.AddAsync(log, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
