using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class SeguroMedicoRepository(FunbideDbContext dbContext) : ISeguroMedicoRepository
{
    public async Task<IReadOnlyList<SeguroMedico>> ObtenerTodosAsync(bool incluirInactivos, CancellationToken cancellationToken)
    {
        var query = dbContext.SegurosMedicos.AsNoTracking().AsQueryable();

        if (!incluirInactivos)
        {
            query = query.Where(s => s.Activo);
        }

        return await query.OrderBy(s => s.Nombre).ToListAsync(cancellationToken);
    }

    public Task<SeguroMedico?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.SegurosMedicos.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    // Case-insensitive (ILike sin comodines, solo para comparar igualdad): sin esto,
    // "ARS Humano" y "ars humano" se consideraban nombres distintos y no se detectaba el
    // duplicado (comparación == de Postgres es sensible a mayúsculas por collation por defecto).
    public Task<SeguroMedico?> ObtenerPorNombreAsync(string nombre, CancellationToken cancellationToken) =>
        dbContext.SegurosMedicos.AsNoTracking().FirstOrDefaultAsync(s => EF.Functions.ILike(s.Nombre, nombre.Trim()), cancellationToken);

    public async Task AgregarAsync(SeguroMedico seguro, CancellationToken cancellationToken) =>
        await dbContext.SegurosMedicos.AddAsync(seguro, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
