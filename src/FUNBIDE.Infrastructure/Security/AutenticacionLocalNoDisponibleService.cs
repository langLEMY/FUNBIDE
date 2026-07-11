using FUNBIDE.Application.Common.Interfaces;

namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Se registra en modo Auth:Provider=Supabase, donde <c>POST /api/auth/login</c> no
/// se usa (el frontend inicia sesión directo contra Supabase Auth). Sin esta
/// implementación de relleno, la validación de DI al arrancar (y "dotnet ef
/// migrations") fallaría porque <c>IniciarSesionLocalUseCase</c> depende de
/// <see cref="IAutenticacionLocalService"/> incondicionalmente.
/// </summary>
public sealed class AutenticacionLocalNoDisponibleService : IAutenticacionLocalService
{
    public Task<TokenLocalResultado?> IniciarSesionAsync(
        string correo, string contrasena, CancellationToken cancellationToken) =>
        Task.FromResult<TokenLocalResultado?>(null);
}
