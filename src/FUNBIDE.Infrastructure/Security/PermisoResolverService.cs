using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Infrastructure.Security;

public sealed class PermisoResolverService(IPermisoUsuarioRepository permisoUsuarioRepository) : IPermisoResolverService
{
    public async Task<IReadOnlySet<ModuloPermiso>> ObtenerPermisosEfectivosAsync(
        Guid supabaseUserId, RolUsuario rol, CancellationToken cancellationToken)
    {
        var efectivos = new HashSet<ModuloPermiso>(PermisosPorRolDefault.Para(rol));
        var overrides = await permisoUsuarioRepository.ObtenerPorSupabaseUserIdAsync(supabaseUserId, cancellationToken);

        foreach (var permiso in overrides)
        {
            if (permiso.Concedido)
            {
                efectivos.Add(permiso.Modulo);
            }
            else
            {
                efectivos.Remove(permiso.Modulo);
            }
        }

        return efectivos;
    }
}
