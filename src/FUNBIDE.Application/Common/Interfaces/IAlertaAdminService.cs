namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>
/// Punto único para avisar a un administrador de un evento crítico que no debería
/// descubrirse por casualidad (ej. el backup automático falló) — hoy solo
/// <c>BackupEjecutorService</c> lo usa. La implementación por defecto (ver
/// <c>AlertaAdminLogService</c> en Infrastructure) solo deja un log imposible de pasar
/// por alto; no hay integración de correo ni WhatsApp todavía en el proyecto, así que
/// implementar esta interfaz con un canal real es agregar un servicio nuevo detrás del
/// mismo punto de llamada, sin tocar quien la usa.
/// </summary>
public interface IAlertaAdminService
{
    Task NotificarFalloAsync(string titulo, string detalle, CancellationToken cancellationToken);
}
