namespace FUNBIDE.Infrastructure.Security;

/// <summary>
/// Opciones del JWT emitido localmente en modo Auth:Provider=Local. A diferencia de
/// <see cref="JwtSupabaseOptions"/>, acá sí hay una clave de firma fija: no hay GoTrue
/// que publique JWKS, así que esta misma API firma y valida con la misma clave
/// simétrica. Generarla una vez por instalación (nunca reutilizar entre instalaciones).
/// </summary>
public sealed class LocalJwtOptions
{
    public const string SeccionConfiguracion = "Auth:Local";

    /// <summary>Clave de firma HMAC en base64 (mínimo 32 bytes decodificados).</summary>
    public required string SigningKeyBase64 { get; init; }

    public string Issuer { get; init; } = "funbide-local";

    public string Audience { get; init; } = "funbide-local";

    /// <summary>
    /// Duración del token emitido. Sin refresh token en modo Local: al vencer, el
    /// usuario vuelve a iniciar sesión. Se usa una duración larga porque es para una
    /// demo/instalación de un solo puesto, no para producción real.
    /// </summary>
    public TimeSpan DuracionToken { get; init; } = TimeSpan.FromHours(12);
}
