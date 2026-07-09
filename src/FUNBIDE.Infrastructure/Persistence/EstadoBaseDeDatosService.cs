using FUNBIDE.Application.Common.Interfaces;

namespace FUNBIDE.Infrastructure.Persistence;

public sealed class EstadoBaseDeDatosService(FunbideDbContext dbContext) : IEstadoBaseDeDatosService
{
    public Task<bool> VerificarConexionAsync(CancellationToken cancellationToken) =>
        dbContext.Database.CanConnectAsync(cancellationToken);
}
