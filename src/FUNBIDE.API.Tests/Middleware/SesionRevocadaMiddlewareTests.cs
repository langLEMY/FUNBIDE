using System.Security.Claims;
using FUNBIDE.API.Middleware;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace FUNBIDE.API.Tests.Middleware;

public class SesionRevocadaMiddlewareTests
{
    private static DefaultHttpContext CrearContexto(ClaimsPrincipal? usuario = null)
    {
        var contexto = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
        };

        if (usuario is not null)
        {
            contexto.User = usuario;
        }

        return contexto;
    }

    private static IMemoryCache CrearCache() => new MemoryCache(new MemoryCacheOptions());

    private static ClaimsPrincipal UsuarioConIat(DateTimeOffset emitidoEn) =>
        new(new ClaimsIdentity([new Claim("iat", emitidoEn.ToUnixTimeSeconds().ToString())], "TestAuth"));

    private static ClaimsPrincipal UsuarioSinIat() =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Admin")], "TestAuth"));

    private static ConfiguracionSistema ConfiguracionConSesionesRevocadasEn(DateTimeOffset momento)
    {
        var configuracion = new ConfiguracionSistema(Guid.NewGuid(), DateTimeOffset.UtcNow);
        configuracion.RevocarSesiones(Guid.NewGuid(), momento);
        return configuracion;
    }

    [Fact]
    public async Task InvokeAsync_UsuarioNoAutenticado_DejaPasar()
    {
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        var contexto = CrearContexto();
        var siguienteInvocado = false;
        var middleware = new SesionRevocadaMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.True(siguienteInvocado);
        await repositorio.DidNotReceive().ObtenerAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_NuncaSeRevocaronSesiones_DejaPasar()
    {
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        repositorio.ObtenerAsync(Arg.Any<CancellationToken>()).Returns((ConfiguracionSistema?)null);
        var contexto = CrearContexto(UsuarioConIat(DateTimeOffset.UtcNow));
        var siguienteInvocado = false;
        var middleware = new SesionRevocadaMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.True(siguienteInvocado);
    }

    [Fact]
    public async Task InvokeAsync_TokenEmitidoAntesDeLaRevocacion_Devuelve401()
    {
        var revocadasEn = DateTimeOffset.UtcNow;
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        repositorio.ObtenerAsync(Arg.Any<CancellationToken>()).Returns(ConfiguracionConSesionesRevocadasEn(revocadasEn));
        var contexto = CrearContexto(UsuarioConIat(revocadasEn.AddMinutes(-5)));
        var siguienteInvocado = false;
        var middleware = new SesionRevocadaMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.False(siguienteInvocado);
        Assert.Equal(StatusCodes.Status401Unauthorized, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_TokenEmitidoDespuesDeLaRevocacion_DejaPasar()
    {
        var revocadasEn = DateTimeOffset.UtcNow;
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        repositorio.ObtenerAsync(Arg.Any<CancellationToken>()).Returns(ConfiguracionConSesionesRevocadasEn(revocadasEn));
        var contexto = CrearContexto(UsuarioConIat(revocadasEn.AddMinutes(5)));
        var siguienteInvocado = false;
        var middleware = new SesionRevocadaMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.True(siguienteInvocado);
    }

    [Fact]
    public async Task InvokeAsync_SesionesRevocadasYTokenSinClaimIat_Devuelve401()
    {
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        repositorio.ObtenerAsync(Arg.Any<CancellationToken>()).Returns(ConfiguracionConSesionesRevocadasEn(DateTimeOffset.UtcNow));
        var contexto = CrearContexto(UsuarioSinIat());
        var siguienteInvocado = false;
        var middleware = new SesionRevocadaMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.False(siguienteInvocado);
        Assert.Equal(StatusCodes.Status401Unauthorized, contexto.Response.StatusCode);
    }
}
