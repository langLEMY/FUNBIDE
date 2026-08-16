using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Sistema;
using Microsoft.Extensions.Configuration;

namespace FUNBIDE.Infrastructure.Persistence;

/// <summary>
/// Mide el disco de "Backup:DirectorioDestino" (donde de verdad pesan los backups); si el
/// respaldo está deshabilitado o sin configurar, cae al disco donde corre la propia API
/// (<see cref="AppContext.BaseDirectory"/>) — igual es útil saber si ese disco anda justo.
/// </summary>
public sealed class EspacioDiscoService(IConfiguration configuration) : IEspacioDiscoService
{
    private const double UmbralEspacioBajoGb = 2.0;

    public Task<EstadoDiscoDto> ObtenerEstadoAsync(CancellationToken cancellationToken)
    {
        var directorioDestino = configuration["Backup:DirectorioDestino"];
        var rutaAMedir = string.IsNullOrWhiteSpace(directorioDestino) ? AppContext.BaseDirectory : directorioDestino;

        var raiz = Path.GetPathRoot(Path.GetFullPath(rutaAMedir));
        var unidad = new DriveInfo(string.IsNullOrWhiteSpace(raiz) ? AppContext.BaseDirectory : raiz);

        var libreGb = Math.Round(unidad.AvailableFreeSpace / 1024d / 1024 / 1024, 1);
        var totalGb = Math.Round(unidad.TotalSize / 1024d / 1024 / 1024, 1);

        return Task.FromResult(new EstadoDiscoDto(unidad.Name, libreGb, totalGb, libreGb < UmbralEspacioBajoGb));
    }
}
