using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class ServicioRepository(FunbideDbContext dbContext) : IServicioRepository
{
    public async Task<IReadOnlyList<Servicio>> ObtenerTodosAsync(bool incluirInactivos, CancellationToken cancellationToken)
    {
        var query = dbContext.Servicios.AsNoTracking().AsQueryable();

        if (!incluirInactivos)
        {
            query = query.Where(s => s.Activo);
        }

        return await query.OrderBy(s => s.Nombre).ToListAsync(cancellationToken);
    }

    public Task<Servicio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Servicios.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Servicio?> ObtenerPorCodigoAsync(string codigo, CancellationToken cancellationToken) =>
        dbContext.Servicios.AsNoTracking().FirstOrDefaultAsync(s => EF.Functions.ILike(s.Codigo, codigo.Trim()), cancellationToken);

    public async Task<IReadOnlyList<Servicio>> ObtenerTodosParaImportarAsync(CancellationToken cancellationToken) =>
        await dbContext.Servicios.ToListAsync(cancellationToken);

    public async Task AgregarAsync(Servicio servicio, CancellationToken cancellationToken) =>
        await dbContext.Servicios.AddAsync(servicio, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
