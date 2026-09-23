using System.Security.Claims;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FUNBIDE.API.Hubs;

/// <summary>
/// Empuja eventos de caja (cobros, movimientos, cierres de turno) a la ventana de Admin en
/// tiempo real. Solo une al grupo <see cref="GrupoAdmins"/> a quien conecta con rol Admin —
/// mismo claim de rol que <c>RoleAuthorizationMiddleware</c> valida para los endpoints REST,
/// leído acá a mano porque un Hub de SignalR no pasa por ese middleware HTTP. No expone
/// métodos invocables por el cliente: es un canal de solo push desde el servidor.
/// </summary>
[Authorize]
public sealed class CajaHub : Hub
{
    public const string GrupoAdmins = "caja-admins";

    public override async Task OnConnectedAsync()
    {
        var rolClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (rolClaim is not null &&
            Enum.TryParse<RolUsuario>(rolClaim, ignoreCase: true, out var rol) &&
            rol == RolUsuario.Admin)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GrupoAdmins);
        }

        await base.OnConnectedAsync();
    }
}
