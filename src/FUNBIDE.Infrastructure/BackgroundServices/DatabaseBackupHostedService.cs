using FUNBIDE.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FUNBIDE.Infrastructure.BackgroundServices;

/// <summary>
/// Dispara <see cref="IBackupEjecutorService"/> cada <see cref="BackupOptions.Intervalo"/>.
/// Toda la lógica real (pg_dump, cifrado, retención, archivo de estado) vive en el
/// servicio inyectado, que también usa el botón "Forzar backup ahora" (LEMY, Mi Perfil).
/// </summary>
public sealed class DatabaseBackupHostedService(
    IOptions<BackupOptions> options,
    IBackupEjecutorService backupEjecutor,
    ILogger<DatabaseBackupHostedService> logger) : BackgroundService
{
    private readonly BackupOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.Intervalo);

        do
        {
            try
            {
                await backupEjecutor.EjecutarAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // BackupEjecutorService ya registró el estado de falla; acá solo evitamos
                // que la excepción tumbe el BackgroundService y corte el ciclo periódico.
                logger.LogError(ex, "El backup automático periódico terminó con error.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
