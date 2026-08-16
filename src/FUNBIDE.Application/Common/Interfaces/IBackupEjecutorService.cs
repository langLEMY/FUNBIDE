namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Ejecuta un respaldo de la base de datos bajo demanda (ver <c>BackupEjecutorService</c>
/// en Infrastructure, que también usa <c>DatabaseBackupHostedService</c> para el respaldo
/// automático periódico — misma lógica, dos disparadores distintos). Lanza si el respaldo
/// falla o si esta instalación lo tiene deshabilitado (<c>Backup:Habilitado=false</c>).
/// </summary>
public interface IBackupEjecutorService
{
    Task EjecutarAsync(CancellationToken cancellationToken);
}
