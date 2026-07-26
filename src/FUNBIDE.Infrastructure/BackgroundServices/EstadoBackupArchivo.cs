namespace FUNBIDE.Infrastructure.BackgroundServices;

/// <summary>
/// Forma del archivo "estado.json" que <see cref="DatabaseBackupHostedService"/> escribe
/// después de cada corrida (éxito o falla) y que <c>EstadoBackupService</c> lee para
/// alimentar el panel "Estado del sistema". Es el único rastro persistente del backup:
/// antes de esto, solo quedaba en el log de Serilog, no consultable por la API.
/// </summary>
internal sealed record EstadoBackupArchivo(DateTimeOffset UltimaEjecucionUtc, bool Exitoso, string? Mensaje);
