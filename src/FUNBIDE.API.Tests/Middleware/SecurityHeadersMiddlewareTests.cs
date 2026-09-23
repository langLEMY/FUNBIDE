using FUNBIDE.API.Middleware;
using Microsoft.AspNetCore.Http;

namespace FUNBIDE.API.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private static DefaultHttpContext CrearContexto() => new() { Response = { Body = new MemoryStream() } };

    [Fact]
    public async Task InvokeAsync_SiempreAgregaLasCabecerasDeSeguridadYDejaPasar()
    {
        var contexto = CrearContexto();
        var siguienteInvocado = false;
        var middleware = new SecurityHeadersMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto);

        Assert.True(siguienteInvocado);
        Assert.Equal("nosniff", contexto.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("DENY", contexto.Response.Headers["X-Frame-Options"]);
        Assert.Equal("strict-origin-when-cross-origin", contexto.Response.Headers["Referrer-Policy"]);
        Assert.Equal("camera=(), microphone=(), geolocation=()", contexto.Response.Headers["Permissions-Policy"]);
    }

    [Fact]
    public async Task InvokeAsync_LaContentSecurityPolicyVaEnModoReportOnly()
    {
        // No debe existir "Content-Security-Policy" (modo bloqueo) todavía — solo la
        // variante "-Report-Only", que no rompe nada mientras se revisan los reportes.
        var contexto = CrearContexto();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(contexto);

        Assert.False(contexto.Response.Headers.ContainsKey("Content-Security-Policy"));
        Assert.True(contexto.Response.Headers.ContainsKey("Content-Security-Policy-Report-Only"));
        Assert.Contains("default-src 'self'", contexto.Response.Headers["Content-Security-Policy-Report-Only"].ToString());
    }
}
