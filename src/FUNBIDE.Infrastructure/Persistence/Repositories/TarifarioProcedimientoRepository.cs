using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class TarifarioProcedimientoRepository(FunbideDbContext dbContext) : ITarifarioProcedimientoRepository
{
    public async Task<IReadOnlyList<TarifarioProcedimiento>> ObtenerPorSeguroYPlanAsync(
        Guid seguroMedicoId, PlanSenasa plan, CancellationToken cancellationToken) =>
        await dbContext.TarifarioProcedimientos
            .AsNoTracking()
            .Where(t => t.SeguroMedicoId == seguroMedicoId && t.Plan == plan && t.Activo)
            .OrderBy(t => t.Procedimiento)
            .ToListAsync(cancellationToken);

    public Task<TarifarioProcedimiento?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.TarifarioProcedimientos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TarifarioProcedimiento>> ObtenerParaImportarAsync(
        Guid seguroMedicoId, PlanSenasa plan, CancellationToken cancellationToken) =>
        await dbContext.TarifarioProcedimientos
            .Where(t => t.SeguroMedicoId == seguroMedicoId && t.Plan == plan)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(TarifarioProcedimiento tarifario, CancellationToken cancellationToken) =>
        await dbContext.TarifarioProcedimientos.AddAsync(tarifario, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
