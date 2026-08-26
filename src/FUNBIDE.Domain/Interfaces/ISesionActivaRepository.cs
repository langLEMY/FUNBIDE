using FUNBIDE.Domain.Entities;

namespace FUNBIDE.Domain.Interfaces;

public interface ISesionActivaRepository
{
    Task<SesionActiva?> ObtenerPorUsuarioYSessionIdAsync(
        Guid usuarioId, string sessionId, CancellationToken cancellationToken);

    /// <summary>Cuenta sesiones (dispositivos) distintas con latido desde <paramref name="desde"/> — la definición de "activa ahora".</summary>
    Task<int> ContarActivasDesdeAsync(DateTimeOffset desde, CancellationToken cancellationToken);

    Task AgregarAsync(SesionActiva sesion, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
