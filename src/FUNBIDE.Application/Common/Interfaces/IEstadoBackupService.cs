namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>Última corrida conocida del backup automático, para el panel "Estado del sistema".</summary>
public sealed record EstadoBackupInfo(DateTimeOffset UltimaEjecucionUtc, bool Exitoso);

/// <summary>
/// Lee el rastro dejado por el backup automático (ver <c>DatabaseBackupHostedService</c>
/// en Infrastructure). Devuelve <c>null</c> si el backup nunca corrió todavía o si está
/// deshabilitado en esta instalación (<c>Backup:Habilitado=false</c>).
/// </summary>
public interface IEstadoBackupService
{
    Task<EstadoBackupInfo?> ObtenerUltimoEstadoAsync(CancellationToken cancellationToken);
}
