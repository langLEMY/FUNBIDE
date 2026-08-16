using System.Security.Claims;
using FUNBIDE.API.Authorization;
using FUNBIDE.API.Middleware;
using FUNBIDE.Domain.Entities;
using FUNBIDE.Domain.Enums;
using FUNBIDE.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace FUNBIDE.API.Tests.Middleware;

public class MantenimientoMiddlewareTests
{
    private static DefaultHttpContext CrearContexto(ClaimsPrincipal? usuario = null, Attribute? metadataEndpoint = null)
    {
        var contexto = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
        };

        if (usuario is not null)
        {
            contexto.User = usuario;
        }

        var metadata = metadataEndpoint is null
            ? new EndpointMetadataCollection()
            : new EndpointMetadataCollection(metadataEndpoint);
        contexto.SetEndpoint(new Endpoint(_ => Task.CompletedTask, metadata, "test"));

        return contexto;
    }

    private static IMemoryCache CrearCache() => new MemoryCache(new MemoryCacheOptions());

    private static ClaimsPrincipal UsuarioConRol(RolUsuario rol) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, rol.ToString())], "TestAuth"));

    private static ConfiguracionSistema ConfiguracionActivaConMensaje(string mensaje)
    {
        var configuracion = new ConfiguracionSistema(Guid.NewGuid(), DateTimeOffset.UtcNow);
        configuracion.CambiarModoMantenimiento(true, mensaje, Guid.NewGuid(), DateTimeOffset.UtcNow);
        return configuracion;
    }

    [Fact]
    public async Task InvokeAsync_MantenimientoActivo_UsuarioNoAutenticado_DejaPasar()
    {
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        var contexto = CrearContexto();
        var siguienteInvocado = false;
        var middleware = new MantenimientoMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.True(siguienteInvocado);
        await repositorio.DidNotReceive().ObtenerAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_MantenimientoActivo_Lemy_DejaPasarSinConsultarConfiguracion()
    {
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        var contexto = CrearContexto(UsuarioConRol(RolUsuario.Lemy));
        var siguienteInvocado = false;
        var middleware = new MantenimientoMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.True(siguienteInvocado);
        await repositorio.DidNotReceive().ObtenerAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_MantenimientoActivo_EndpointVisibleDuranteMantenimiento_DejaPasar()
    {
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        repositorio.ObtenerAsync(Arg.Any<CancellationToken>()).Returns(ConfiguracionActivaConMensaje("En mantenimiento"));
        var contexto = CrearContexto(UsuarioConRol(RolUsuario.Admin), new VisibleDuranteMantenimientoAttribute());
        var siguienteInvocado = false;
        var middleware = new MantenimientoMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.True(siguienteInvocado);
    }

    [Fact]
    public async Task InvokeAsync_MantenimientoActivo_RolNoLemySinExencion_Devuelve503ConElMensaje()
    {
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        repositorio.ObtenerAsync(Arg.Any<CancellationToken>()).Returns(ConfiguracionActivaConMensaje("Migrando la base de datos"));
        var contexto = CrearContexto(UsuarioConRol(RolUsuario.Admin));
        var siguienteInvocado = false;
        var middleware = new MantenimientoMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.False(siguienteInvocado);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_MantenimientoApagado_DejaPasarACualquierRol()
    {
        var repositorio = Substitute.For<IConfiguracionSistemaRepository>();
        repositorio.ObtenerAsync(Arg.Any<CancellationToken>()).Returns((ConfiguracionSistema?)null);
        var contexto = CrearContexto(UsuarioConRol(RolUsuario.Fondos));
        var siguienteInvocado = false;
        var middleware = new MantenimientoMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto, repositorio, CrearCache());

        Assert.True(siguienteInvocado);
    }
}
