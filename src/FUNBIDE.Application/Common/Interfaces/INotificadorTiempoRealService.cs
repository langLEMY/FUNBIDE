namespace FUNBIDE.Application.Common.Interfaces;

/// <summary>Un evento de caja para empujar a la ventana de Admin en tiempo real (ver <c>CajaHub</c>).</summary>
public sealed record EventoCajaDto(
    string Tipo, // "cobro" | "movimiento" | "turno-cerrado"
    string Descripcion,
    decimal Monto,
    bool EsIngreso,
    DateTimeOffset RegistradoEn);

/// <summary>
/// Empuja eventos de caja a los admins conectados vía SignalR (<c>CajaHub</c>), para que
/// el resumen y el timeline de <c>CajaPage</c> se actualicen al instante en vez de esperar
/// el sondeo de 20s. La implementación vive en la capa API (donde vive el Hub); esta capa
/// solo conoce la interfaz. Nunca debe hacer fallar el caso de uso que la llama: cada
/// llamador la invoca "best-effort" después de confirmar su propia transacción.
/// </summary>
public interface INotificadorTiempoRealService
{
    Task NotificarEventoCajaAsync(EventoCajaDto evento, CancellationToken cancellationToken);
}
