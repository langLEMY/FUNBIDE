using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class SesionActivaRepository(FunbideDbContext dbContext) : ISesionActivaRepository
{
    public Task<SesionActiva?> ObtenerPorUsuarioYSessionIdAsync(
        Guid usuarioId, string sessionId, CancellationToken cancellationToken) =>
        dbContext.SesionesActivas.FirstOrDefaultAsync(
            s => s.UsuarioId == usuarioId && s.SessionId == sessionId, cancellationToken);

    public Task<int> ContarActivasDesdeAsync(DateTimeOffset desde, CancellationToken cancellationToken) =>
        dbContext.SesionesActivas.AsNoTracking().CountAsync(s => s.UltimoVistoEn >= desde, cancellationToken);

    public async Task AgregarAsync(SesionActiva sesion, CancellationToken cancellationToken) =>
        await dbContext.SesionesActivas.AddAsync(sesion, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
