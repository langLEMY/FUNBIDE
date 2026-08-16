using System.Text.Json;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Infrastructure.BackgroundServices;
using Microsoft.Extensions.Configuration;

namespace FUNBIDE.Infrastructure.Persistence;

/// <summary>
/// Lee directamente de <see cref="IConfiguration"/> (no de <c>IOptions&lt;BackupOptions&gt;</c>,
/// que solo se registra cuando <c>Backup:Habilitado=true</c>) para poder responder "nunca
/// corrió / deshabilitado" en instalaciones offline sin lanzar al arrancar.
/// </summary>
public sealed class EstadoBackupService(IConfiguration configuration) : IEstadoBackupService
{
    public async Task<EstadoBackupInfo?> ObtenerUltimoEstadoAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue("Backup:Habilitado", true))
        {
            return null;
        }

        var directorioDestino = configuration["Backup:DirectorioDestino"];
        if (string.IsNullOrWhiteSpace(directorioDestino))
        {
            return null;
        }

        var rutaEstado = Path.Combine(directorioDestino, BackupEjecutorService.NombreArchivoEstado);
        if (!File.Exists(rutaEstado))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(rutaEstado);
            var estado = await JsonSerializer.DeserializeAsync<EstadoBackupArchivo>(stream, cancellationToken: cancellationToken);
            return estado is null ? null : new EstadoBackupInfo(estado.UltimaEjecucionUtc, estado.Exitoso);
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return null;
        }
    }
}
