using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

/// <summary>
/// Igual que <see cref="IHistorialClinicoRepository"/>, sin operaciones de mutación:
/// la auditoría solo se inserta y se consulta.
/// </summary>
public interface IAuditoriaLogRepository
{
    /// <summary>
    /// <paramref name="desde"/>/<paramref name="hasta"/> son obligatorios acá a propósito
    /// (ver ObtenerLogsAuditoriaUseCase, que les aplica un default de "últimos 30 días" y
    /// un span máximo antes de llegar a este método) — la tabla de auditoría crece sin
    /// límite, así que a diferencia de <see cref="IPacienteRepository.ObtenerPaginadoAsync"/>
    /// no alcanza con paginar, también hay que acotar el rango de fechas siempre.
    /// </summary>
    Task<(IReadOnlyList<AuditoriaLog> Items, int Total)> ObtenerPaginadoAsync(
        DateTimeOffset desde, DateTimeOffset hasta, string? recurso, int pagina, int tamanoPagina,
        CancellationToken cancellationToken);

    Task RegistrarAsync(AuditoriaLog log, CancellationToken cancellationToken);
}
