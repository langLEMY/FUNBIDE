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
}
