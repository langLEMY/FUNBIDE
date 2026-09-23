namespace FUNBIDE.API.Middleware;

/// <summary>
/// Cabeceras de seguridad HTTP aplicadas a TODA respuesta — incluida la del frontend
/// servido como archivos estáticos (por eso corre antes de UseRouting, no solo alrededor
/// de los controladores de la API). Complementa, no reemplaza: UseHsts ya fuerza HTTPS
/// (Program.cs, activo en modo Production — Dockerfile/docker-compose.yml lo fijan así
/// para el despliegue real), CORS ya tiene allow-list explícito (CorsExtensions, nunca
/// AllowAnyOrigin) y el rate limiter ya está particionado por IP.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Punto de partida deliberadamente permisivo en script-src/style-src ('unsafe-inline'):
    /// el <c>&lt;script&gt;</c> inline de index.html que evita el parpadeo de tema
    /// claro/oscuro al cargar, y los estilos inline que React genera vía la prop `style`,
    /// no tienen nonce ni hash — cerrarlos de golpe generaría ruido en vez de señal. Content-
    /// Security-Policy-Report-Only no bloquea nada, solo reporta violaciones en la consola
    /// del navegador (DevTools > Console/Network): revisarlas ahí antes de pasar a
    /// Content-Security-Policy real (bloqueo), que es el siguiente paso una vez que la
    /// política esté afinada contra el uso real de la app.
    /// </summary>
    private const string PoliticaContentSecurity =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self' https://*.supabase.co wss://*.supabase.co; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "object-src 'none'";

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // MIME sniffing: sin esto, un navegador puede decidir por su cuenta que un archivo
        // servido como texto/imagen es en realidad JS/HTML y ejecutarlo.
        headers["X-Content-Type-Options"] = "nosniff";

        // La app nunca se embebe en un <iframe> de otro sitio — bloquea clickjacking.
        headers["X-Frame-Options"] = "DENY";

        // Manda el origen completo solo a nuestro propio dominio; a terceros (ej. si algún
        // día hay un link saliente), solo el origen, nunca la ruta completa con posibles
        // ids de paciente/cita en la URL.
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Ningún módulo de la app usa cámara/micrófono/geolocalización (verificado: sin
        // getUserMedia/mediaDevices/geolocation en el frontend) — los tres bloqueados por
        // completo, no hace falta dejarlos abiertos "por si acaso".
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        headers["Content-Security-Policy-Report-Only"] = PoliticaContentSecurity;

        await next(context);
    }
}
