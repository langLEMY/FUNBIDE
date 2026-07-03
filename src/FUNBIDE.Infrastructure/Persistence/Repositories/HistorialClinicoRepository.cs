using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación fiel al contrato append-only: solo expone inserción y lectura.
/// </summary>
public sealed class HistorialClinicoRepository(FunbideDbContext dbContext) : IHistorialClinicoRepository
{
    public async Task<IReadOnlyList<EntradaHistorialClinico>> ObtenerPorPacienteAsync(
        Guid pacienteId, CancellationToken cancellationToken) =>
        await dbContext.HistorialClinico
            .AsNoTracking()
            .Where(e => e.PacienteId == pacienteId)
            .ToListAsync(cancellationToken);

    public async Task RegistrarAsync(EntradaHistorialClinico entrada, CancellationToken cancellationToken)
    {
        await dbContext.HistorialClinico.AddAsync(entrada, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
