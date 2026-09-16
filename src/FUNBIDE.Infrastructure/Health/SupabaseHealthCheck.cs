using FUNBIDE.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FUNBIDE.Infrastructure.Health;

/// <summary>
/// Antes /health solo confirmaba que el proceso de la API estaba arriba, sin decir nada
/// de si Postgres/Supabase era alcanzable — un contenedor "sano" según Docker podía estar
/// completamente inutilizable si la base de datos se caía. Este check hace un
/// <see cref="DbContext.CanConnectAsync"/> real, con un timeout corto (no el
/// CommandTimeout de 30s de las queries normales: acá solo importa "¿responde o no?", no
/// vale la pena esperar tanto para eso).
/// </summary>
public sealed class SupabaseHealthCheck(FunbideDbContext dbContext) : IHealthCheck
{
    private static readonly TimeSpan TimeoutConexion = TimeSpan.FromSeconds(5);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeoutConexion);

        try
        {
            var puedeConectar = await dbContext.Database.CanConnectAsync(cts.Token);
            return puedeConectar
                ? HealthCheckResult.Healthy("Conexión a la base de datos verificada.")
                : HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy($"La base de datos no respondió dentro de {TimeoutConexion.TotalSeconds}s.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.", ex);
        }
    }
}
