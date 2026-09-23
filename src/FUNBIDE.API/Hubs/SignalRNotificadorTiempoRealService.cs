using FUNBIDE.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FUNBIDE.API.Hubs;

/// <summary>
/// Implementación de <see cref="INotificadorTiempoRealService"/> sobre <see cref="IHubContext{CajaHub}"/>.
/// Vive en la capa API (donde vive el Hub) aunque la interfaz que implementa es de
/// Application — mismo patrón de "puerto en Application, adaptador donde corresponda" que
/// el resto del proyecto, salvo que acá el adaptador natural es la capa de transporte
/// (API), no Infrastructure.
/// </summary>
public sealed class SignalRNotificadorTiempoRealService(IHubContext<CajaHub> hubContext) : INotificadorTiempoRealService
{
    public Task NotificarEventoCajaAsync(EventoCajaDto evento, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(CajaHub.GrupoAdmins).SendAsync("evento-caja", evento, cancellationToken);
}
