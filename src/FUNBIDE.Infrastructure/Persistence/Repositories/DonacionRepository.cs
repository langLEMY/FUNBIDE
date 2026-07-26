using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class DonacionRepository(FunbideDbContext dbContext) : IDonacionRepository
{
    public async Task<IReadOnlyList<Donacion>> ObtenerPorRangoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, CancellationToken cancellationToken) =>
        await dbContext.Donaciones
            .AsNoTracking()
            .Where(d => d.RegistradoEn >= desde && d.RegistradoEn < hasta)
            .OrderByDescending(d => d.RegistradoEn)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(Donacion donacion, CancellationToken cancellationToken) =>
        await dbContext.Donaciones.AddAsync(donacion, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
