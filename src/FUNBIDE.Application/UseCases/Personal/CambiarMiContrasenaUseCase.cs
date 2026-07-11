using FUNBIDE.Application.Common;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Application.DTOs.Personal;

namespace FUNBIDE.Application.UseCases.Personal;

public interface ICambiarMiContrasenaUseCase : IUseCase<CambiarMiContrasenaRequest, bool>
{
}

/// <summary>
/// Autoservicio: cualquier usuario autenticado cambia su propia contraseña — a
/// diferencia de <see cref="CambiarContrasenaUsuarioUseCase"/> (solo Lemy, le cambia
/// la contraseña a otro). En modo Auth:Provider=Supabase esto no se usa: el frontend
/// llama a supabase.auth.updateUser directo, sin pasar por el backend. En modo Local
/// sí hace falta, porque no hay Supabase que lo resuelva del lado del cliente.
/// </summary>
public sealed class CambiarMiContrasenaUseCase(
    ISupabaseAdminService supabaseAdmin,
    ICurrentUserService currentUser) : ICambiarMiContrasenaUseCase
{
    public async Task<bool> EjecutarAsync(CambiarMiContrasenaRequest request, CancellationToken cancellationToken)
    {
        await supabaseAdmin.CambiarContrasenaAsync(currentUser.UsuarioId, request.NuevaContrasena, cancellationToken);
        return true;
    }
}
