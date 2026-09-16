using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class AuditoriaLogRepository(FunbideDbContext dbContext) : IAuditoriaLogRepository
{
    public async Task<(IReadOnlyList<AuditoriaLog> Items, int Total)> ObtenerPaginadoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, string? recurso, int pagina, int tamanoPagina,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuditoriaLogs.AsNoTracking()
            .Where(l => l.RegistradoEn >= desde && l.RegistradoEn <= hasta);

        if (!string.IsNullOrWhiteSpace(recurso))
        {
            query = query.Where(l => l.Recurso == recurso);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.RegistradoEn)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task RegistrarAsync(AuditoriaLog log, CancellationToken cancellationToken)
    {
        await dbContext.AuditoriaLogs.AddAsync(log, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
