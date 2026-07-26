using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FUNBIDE.Infrastructure.Persistence.Repositories;

public sealed class ConfiguracionSistemaRepository(FunbideDbContext dbContext) : IConfiguracionSistemaRepository
{
    // Sin AsNoTracking a propósito: tanto la lectura para el middleware/panel como la
    // escritura del toggle pasan por este mismo método, y la escritura necesita la
    // entidad rastreada para que SaveChangesAsync detecte el cambio.
    public Task<ConfiguracionSistema?> ObtenerAsync(CancellationToken cancellationToken) =>
        dbContext.ConfiguracionSistema.FirstOrDefaultAsync(cancellationToken);

    public async Task AgregarAsync(ConfiguracionSistema configuracion, CancellationToken cancellationToken) =>
        await dbContext.ConfiguracionSistema.AddAsync(configuracion, cancellationToken);

    public Task GuardarCambiosAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
