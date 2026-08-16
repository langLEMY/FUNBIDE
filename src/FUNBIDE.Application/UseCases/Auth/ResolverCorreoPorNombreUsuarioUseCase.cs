using FUNBIDE.Application.Common;
using FUNBIDE.Application.DTOs.Auth;
using FUNBIDE.Domain.Interfaces;

namespace FUNBIDE.Application.UseCases.Auth;

public interface IResolverCorreoPorNombreUsuarioUseCase : IUseCase<string, ResolverCorreoDto>
{
}

/// <summary>
/// Traduce el nombre de usuario que la persona escribe en el login al correo real que
/// necesita el paso siguiente (Supabase Auth en modo nube, o el propio backend en modo
/// Local) — el login sigue validándose por correo+contraseña en ambos casos, esto solo
/// resuelve el identificador antes de llegar ahí.
/// </summary>
public sealed class ResolverCorreoPorNombreUsuarioUseCase(IUsuarioRepository usuarioRepository)
    : IResolverCorreoPorNombreUsuarioUseCase
{
    public async Task<ResolverCorreoDto> EjecutarAsync(string nombreUsuario, CancellationToken cancellationToken)
    {
        var normalizado = nombreUsuario.Trim().ToLowerInvariant();
        var usuario = await usuarioRepository.ObtenerPorNombreUsuarioAsync(normalizado, cancellationToken);

        // Cuando el nombre de usuario no existe, devolvemos un correo con formato válido
        // pero inexistente en vez de un 404: así el intento de login sigue exactamente el
        // mismo camino tanto si el usuario existe como si no (Supabase/el login local
        // responden "credenciales inválidas" en ambos casos), y nadie puede usar este
        // endpoint para enumerar qué nombres de usuario están registrados.
        var correo = usuario?.Correo ?? $"{normalizado}@no-existe.funbide.invalid";
        return new ResolverCorreoDto(correo);
    }
}
