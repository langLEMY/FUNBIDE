using FUNBIDE.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FUNBIDE.Infrastructure.Logging;

/// <summary>
/// Implementación por defecto de <see cref="IAlertaAdminService"/>: sin correo ni WhatsApp
/// configurados todavía, la única forma de "avisar" es dejar un log de nivel Critical con
/// un marcador propio ("ALERTA_ADMIN") fácil de grepear/alertar desde afuera (Docker logs,
/// un agregador). No reemplaza una notificación push real, pero es mejor que lo que había
/// antes: un archivo de estado que nadie mira hasta el día que hace falta restaurar.
/// </summary>
public sealed class AlertaAdminLogService(ILogger<AlertaAdminLogService> logger) : IAlertaAdminService
{
    public Task NotificarFalloAsync(string titulo, string detalle, CancellationToken cancellationToken)
    {
        logger.LogCritical("ALERTA_ADMIN: {Titulo} — {Detalle}", titulo, detalle);
        return Task.CompletedTask;
    }
}
