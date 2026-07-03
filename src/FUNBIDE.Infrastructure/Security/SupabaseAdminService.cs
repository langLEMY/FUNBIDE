using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Enums;

namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Implementa <see cref="ISupabaseAdminService"/> contra la GoTrue Admin API
/// (/auth/v1/admin/users) usando un <see cref="HttpClient"/> ya configurado con la
/// URL base y las cabeceras de autenticación del service role key (ver
/// <see cref="DependencyInjection.AddFunbideInfrastructure"/>).
/// </summary>
public sealed class SupabaseAdminService(HttpClient httpClient) : ISupabaseAdminService
{
    public async Task<Guid> CrearUsuarioAsync(
        string correo, string contrasenaTemporal, RolUsuario rol, CancellationToken cancellationToken)
    {
        var payload = new CrearUsuarioPayload(correo, contrasenaTemporal, true, new RolPayload(rol.ToString()));

        using var respuesta = await httpClient.PostAsJsonAsync("users", payload, cancellationToken);
        await LanzarSiFallaAsync(respuesta, "crear el usuario", cancellationToken);

        var usuarioCreado = await respuesta.Content.ReadFromJsonAsync<UsuarioSupabaseRespuesta>(cancellationToken)
            ?? throw new InvalidOperationException("Supabase Auth no devolvió el usuario creado.");

        return usuarioCreado.Id;
    }

    public async Task CambiarRolAsync(Guid supabaseUserId, RolUsuario nuevoRol, CancellationToken cancellationToken)
    {
        var payload = new CambiarRolPayload(new RolPayload(nuevoRol.ToString()));

        using var respuesta = await httpClient.PutAsJsonAsync($"users/{supabaseUserId}", payload, cancellationToken);
        await LanzarSiFallaAsync(respuesta, "cambiar el rol del usuario", cancellationToken);
    }

    public async Task CambiarContrasenaAsync(Guid supabaseUserId, string nuevaContrasena, CancellationToken cancellationToken)
    {
        var payload = new CambiarContrasenaPayload(nuevaContrasena);

        using var respuesta = await httpClient.PutAsJsonAsync($"users/{supabaseUserId}", payload, cancellationToken);
        await LanzarSiFallaAsync(respuesta, "cambiar la contraseña del usuario", cancellationToken);
    }

    public async Task CambiarCorreoAsync(Guid supabaseUserId, string nuevoCorreo, CancellationToken cancellationToken)
    {
        var payload = new CambiarCorreoPayload(nuevoCorreo, true);

        using var respuesta = await httpClient.PutAsJsonAsync($"users/{supabaseUserId}", payload, cancellationToken);
        await LanzarSiFallaAsync(respuesta, "cambiar el correo del usuario", cancellationToken);
    }

    public async Task RevocarAccesoAsync(Guid supabaseUserId, CancellationToken cancellationToken)
    {
        // ban_duration grande en lugar de DELETE: revoca el acceso de forma reversible
        // sin destruir el usuario en Supabase Auth ni romper la trazabilidad histórica.
        var payload = new BanDuracionPayload("87600h");

        using var respuesta = await httpClient.PutAsJsonAsync($"users/{supabaseUserId}", payload, cancellationToken);
        await LanzarSiFallaAsync(respuesta, "revocar el acceso del usuario", cancellationToken);
    }

    public async Task RestaurarAccesoAsync(Guid supabaseUserId, CancellationToken cancellationToken)
    {
        var payload = new BanDuracionPayload("none");

        using var respuesta = await httpClient.PutAsJsonAsync($"users/{supabaseUserId}", payload, cancellationToken);
        await LanzarSiFallaAsync(respuesta, "restaurar el acceso del usuario", cancellationToken);
    }

    public async Task EliminarPermanentementeAsync(Guid supabaseUserId, CancellationToken cancellationToken)
    {
        using var respuesta = await httpClient.DeleteAsync($"users/{supabaseUserId}", cancellationToken);

        // 404 = el usuario ya no existe en Supabase Auth (se borró por otra vía, o esta
        // es una segunda llamada). El objetivo — que no pueda iniciar sesión — ya está
        // cumplido, así que se trata como éxito idempotente en vez de fallar la operación.
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return;
        }

        await LanzarSiFallaAsync(respuesta, "eliminar permanentemente el usuario", cancellationToken);
    }

    private static async Task LanzarSiFallaAsync(
        HttpResponseMessage respuesta, string accion, CancellationToken cancellationToken)
    {
        if (respuesta.IsSuccessStatusCode)
        {
            return;
        }

        var detalle = await respuesta.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"No se pudo {accion} en Supabase Auth (HTTP {(int)respuesta.StatusCode}): {detalle}");
    }

    private sealed record CrearUsuarioPayload(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("password")] string Password,
        [property: JsonPropertyName("email_confirm")] bool EmailConfirm,
        [property: JsonPropertyName("app_metadata")] RolPayload AppMetadata);

    private sealed record CambiarRolPayload([property: JsonPropertyName("app_metadata")] RolPayload AppMetadata);

    private sealed record CambiarContrasenaPayload([property: JsonPropertyName("password")] string Password);

    private sealed record CambiarCorreoPayload(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("email_confirm")] bool EmailConfirm);

    private sealed record BanDuracionPayload([property: JsonPropertyName("ban_duration")] string BanDuration);

    private sealed record RolPayload([property: JsonPropertyName("role")] string Role);

    private sealed record UsuarioSupabaseRespuesta([property: JsonPropertyName("id")] Guid Id);
}
