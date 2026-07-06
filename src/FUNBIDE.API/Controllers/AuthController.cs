using FUNBIDE.Application.DTOs.Auth;
using FUNBIDE.Application.UseCases.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNBIDE.API.Controllers;

/// <summary>
/// El login en sí ocurre directamente contra Supabase Auth desde el navegador, sin
/// pasar por esta API, así que un intento fallido nunca trae un JWT válido. Este
/// endpoint es deliberadamente anónimo para poder registrar tanto éxitos como
/// fallos; la IP y el dispositivo se extraen de la petición HTTP misma (nunca del
/// cuerpo enviado por el cliente) para que no se puedan falsificar.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(IRegistrarEventoLoginUseCase registrarEventoLogin) : ControllerBase
{
    [HttpPost("eventos-login")]
    [AllowAnonymous]
    public async Task<IActionResult> RegistrarEventoLoginAsync(
        RegistrarEventoLoginRequest cuerpo, CancellationToken cancellationToken)
    {
        var comando = new RegistrarEventoLoginComando(
            cuerpo.Correo,
            cuerpo.Exitoso,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            ResumirDispositivo(Request.Headers.UserAgent.ToString()));

        await registrarEventoLogin.EjecutarAsync(comando, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Heurística simple de "Navegador · SO" a partir del User-Agent, sin depender de
    /// una librería de parsing: el orden de las comprobaciones importa porque Edge y
    /// Opera incluyen "Chrome" en su User-Agent, y Chrome a su vez incluye "Safari".
    /// </summary>
    private static string ResumirDispositivo(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return "Desconocido";
        }

        string navegador = userAgent switch
        {
            var ua when ua.Contains("Edg/") => "Edge",
            var ua when ua.Contains("OPR/") || ua.Contains("Opera") => "Opera",
            var ua when ua.Contains("Chrome/") => "Chrome",
            var ua when ua.Contains("Firefox/") => "Firefox",
            var ua when ua.Contains("Safari/") => "Safari",
            _ => "Navegador desconocido",
        };

        string so = userAgent switch
        {
            var ua when ua.Contains("Windows") => "Windows",
            var ua when ua.Contains("Mac OS X") => "macOS",
            var ua when ua.Contains("Android") => "Android",
            var ua when ua.Contains("iPhone") || ua.Contains("iPad") => "iOS",
            var ua when ua.Contains("Linux") => "Linux",
            _ => "SO desconocido",
        };

        return $"{navegador} · {so}";
    }
}
