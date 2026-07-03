using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FUNBIDE.Infrastructure.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que ejecuta pg_dump periódicamente y cifra el volcado
/// resultante con AES-256 antes de dejarlo en <see cref="BackupOptions.DirectorioDestino"/>.
/// El dump en texto plano se elimina inmediatamente después de cifrarse.
/// </summary>
public sealed class DatabaseBackupHostedService(
    IOptions<BackupOptions> options,
    AesBackupEncryptor encryptor,
    ILogger<DatabaseBackupHostedService> logger) : BackgroundService
{
    private readonly BackupOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_options.DirectorioDestino);

        using var timer = new PeriodicTimer(_options.Intervalo);

        do
        {
            try
            {
                await EjecutarBackupAsync(stoppingToken);
                LimpiarBackupsAntiguos();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falló la ejecución del backup automático de la base de datos.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task EjecutarBackupAsync(CancellationToken cancellationToken)
    {
        var marcaTiempo = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
        var rutaDump = Path.Combine(_options.DirectorioDestino, $"funbide_{marcaTiempo}.dump");

        logger.LogInformation("Iniciando backup de base de datos hacia {RutaDump}", rutaDump);

        var procesoInfo = new ProcessStartInfo
        {
            FileName = _options.PgDumpPath,
            ArgumentList =
            {
                "-h", _options.PgHost,
                "-p", _options.PgPort.ToString(),
                "-U", _options.PgUser,
                "-d", _options.PgDatabase,
                "-F", "c",
                "-f", rutaDump
            },
            RedirectStandardError = true,
            UseShellExecute = false
        };
        procesoInfo.Environment["PGPASSWORD"] = _options.PgPassword;

        using var proceso = Process.Start(procesoInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar el proceso pg_dump.");

        var stderr = await proceso.StandardError.ReadToEndAsync(cancellationToken);
        await proceso.WaitForExitAsync(cancellationToken);

        if (proceso.ExitCode != 0)
        {
            throw new InvalidOperationException($"pg_dump terminó con código {proceso.ExitCode}: {stderr}");
        }

        var claveAes = Convert.FromBase64String(_options.AesKeyBase64);
        var rutaCifrada = await encryptor.CifrarArchivoAsync(rutaDump, claveAes, cancellationToken);

        File.Delete(rutaDump);

        logger.LogInformation("Backup cifrado generado correctamente en {RutaCifrada}", rutaCifrada);
    }

    private void LimpiarBackupsAntiguos()
    {
        var backups = new DirectoryInfo(_options.DirectorioDestino)
            .GetFiles("funbide_*.dump.enc")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Skip(_options.BackupsARetener);

        foreach (var backup in backups)
        {
            backup.Delete();
            logger.LogInformation("Backup antiguo eliminado por política de retención: {Archivo}", backup.Name);
        }
    }
}
