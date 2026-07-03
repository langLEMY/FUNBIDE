using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

/// <summary>
/// Igual que <see cref="IHistorialClinicoRepository"/>, sin operaciones de mutación:
/// la auditoría solo se inserta y se consulta.
/// </summary>
public interface IAuditoriaLogRepository
{
    Task<IReadOnlyList<AuditoriaLog>> ObtenerAsync(
        DateTimeOffset? desde, DateTimeOffset? hasta, string? recurso, CancellationToken cancellationToken);

    Task RegistrarAsync(AuditoriaLog log, CancellationToken cancellationToken);
}
