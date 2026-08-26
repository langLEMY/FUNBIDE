using FUNBIDE.Domain.Common;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Latido de presencia de un dispositivo autenticado — no es un historial de login, es la
/// última vez que se supo que ese dispositivo seguía con la app abierta (ver
/// <c>RegistrarLatidoUseCase</c>). El auth acá es JWT sin estado, así que no hay ningún otro
/// lugar donde el servidor sepa "quién está conectado ahora"; esta tabla es justamente para
/// eso. <see cref="SessionId"/> es un id aleatorio que el frontend genera una sola vez por
/// dispositivo y guarda en localStorage — no es el JWT en sí, así que sobrevive a que el
/// token se renueve.
/// </summary>
public sealed class SesionActiva : Entity
{
    public Guid UsuarioId { get; private set; }
    public string SessionId { get; private set; } = string.Empty;
    public DateTimeOffset UltimoVistoEn { get; private set; }

    private SesionActiva() { }

    public SesionActiva(Guid usuarioId, string sessionId, DateTimeOffset ahora)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("El id de sesión es obligatorio.", nameof(sessionId));
        }

        UsuarioId = usuarioId;
        SessionId = sessionId.Trim();
        UltimoVistoEn = ahora;
    }

    public void RegistrarLatido(DateTimeOffset ahora) => UltimoVistoEn = ahora;
}
