using FUNBIDE.Domain.Common;

namespace FUNBIDE.Domain.Entities;

/// <summary>
/// Fila única (singleton) con los interruptores globales del sistema. Hoy solo el modo
/// mantenimiento; el repositorio devuelve <c>null</c> hasta que alguien lo active por
/// primera vez — no hace falta sembrarla vacía, "no existe fila" ya significa "todo apagado".
/// </summary>
public sealed class ConfiguracionSistema : Entity
{
    public bool ModoMantenimientoActivo { get; private set; }

    /// <summary>Mensaje mostrado a quien quede bloqueado. Se limpia solo al desactivar el modo.</summary>
    public string? ModoMantenimientoMensaje { get; private set; }

    /// <summary>
    /// Marca de tiempo de "reiniciar servicios" (ver <c>ReiniciarServiciosUseCase</c>): cualquier
    /// JWT emitido antes de este momento (claim "iat") se rechaza con 401, forzando un login
    /// nuevo. <c>null</c> = nunca se usó, no se rechaza nada por esta vía.
    /// </summary>
    public DateTimeOffset? SesionesRevocadasEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }
    public Guid ActualizadoPorUsuarioId { get; private set; }

    private ConfiguracionSistema() { }

    public ConfiguracionSistema(Guid actualizadoPorUsuarioId, DateTimeOffset ahora)
    {
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        ActualizadoEn = ahora;
    }

    public void CambiarModoMantenimiento(bool activo, string? mensaje, Guid actualizadoPorUsuarioId, DateTimeOffset ahora)
    {
        ModoMantenimientoActivo = activo;
        ModoMantenimientoMensaje = activo && !string.IsNullOrWhiteSpace(mensaje) ? mensaje.Trim() : null;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        ActualizadoEn = ahora;
    }

    /// <summary>
    /// "Reiniciar servicios": cualquier sesión (JWT) emitida antes de <paramref name="ahora"/>,
    /// incluida la de quien ejecuta esta acción, deja de servir en la próxima request.
    /// </summary>
    public void RevocarSesiones(Guid actualizadoPorUsuarioId, DateTimeOffset ahora)
    {
        SesionesRevocadasEn = ahora;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        ActualizadoEn = ahora;
    }
}
