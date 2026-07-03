using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class ResumenDiarioRepository(FunbideDbContext dbContext) : IResumenDiarioRepository
{
    public async Task<ResumenDiario> ObtenerOCrearConBloqueoAsync(DateOnly fecha, CancellationToken cancellationToken)
    {
        var resultados = await dbContext.ResumenesDiarios
            .FromSqlInterpolated($"SELECT *, xmin FROM funbide.resumenes_diarios WHERE \"Fecha\" = {fecha} FOR UPDATE")
            .ToListAsync(cancellationToken);

        var resumen = resultados.SingleOrDefault();
        if (resumen is not null)
        {
            return resumen;
        }

        resumen = new ResumenDiario(fecha);
        await dbContext.ResumenesDiarios.AddAsync(resumen, cancellationToken);
        return resumen;
    }

    public Task<ResumenDiario?> ObtenerPorFechaAsync(DateOnly fecha, CancellationToken cancellationToken) =>
        dbContext.ResumenesDiarios.AsNoTracking().FirstOrDefaultAsync(r => r.Fecha == fecha, cancellationToken);

    public async Task<IReadOnlyList<ResumenDiario>> ObtenerPorMesAsync(
        int anio, int mes, CancellationToken cancellationToken) =>
        await dbContext.ResumenesDiarios
            .AsNoTracking()
            .Where(r => r.Fecha.Year == anio && r.Fecha.Month == mes)
            .OrderBy(r => r.Fecha)
            .ToListAsync(cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
