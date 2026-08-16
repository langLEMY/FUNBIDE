using FUNBIDE.Application.Common.Interfaces;

namespace FUNBIDE.Infrastructure.BackgroundServices;

/// <summary>
/// Se registra cuando "Backup:Habilitado=false" (instalaciones sin pg_dump/destino
/// configurado). Sin esta implementación de relleno, <c>EjecutarBackupManualUseCase</c>
/// no podría resolver <see cref="IBackupEjecutorService"/> por DI en esas instalaciones.
/// </summary>
public sealed class BackupEjecutorNoDisponibleService : IBackupEjecutorService
{
    public Task EjecutarAsync(CancellationToken cancellationToken) =>
        throw new InvalidOperationException("El respaldo automático está deshabilitado en esta instalación.");
}
