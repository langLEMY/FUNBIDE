using System.Security.Claims;
using FUNBIDE.API.Authorization;
using FUNBIDE.API.Middleware;
using FUNBIDE.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FUNBIDE.API.Tests.Middleware;

public class RoleAuthorizationMiddlewareTests
{
    private static DefaultHttpContext CrearContexto(RequiereRolAttribute? requisito, ClaimsPrincipal? usuario = null)
    {
        var contexto = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
        };

        if (usuario is not null)
        {
            contexto.User = usuario;
        }

        var metadata = requisito is null
            ? new EndpointMetadataCollection()
            : new EndpointMetadataCollection(requisito);
        contexto.SetEndpoint(new Endpoint(_ => Task.CompletedTask, metadata, "test"));

        return contexto;
    }

    private static ClaimsPrincipal UsuarioAutenticadoConRol(RolUsuario rol) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, rol.ToString())], "TestAuth"));

    [Fact]
    public async Task InvokeAsync_SinAtributoRequiereRol_DejaPasarSinImportarAutenticacion()
    {
        var contexto = CrearContexto(requisito: null);
        var siguienteInvocado = false;
        var middleware = new RoleAuthorizationMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto);

        Assert.True(siguienteInvocado);
        Assert.Equal(200, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_NoAutenticado_Devuelve401YNoLlamaASiguiente()
    {
        var contexto = CrearContexto(new RequiereRolAttribute(RolUsuario.Admin));
        var siguienteInvocado = false;
        var middleware = new RoleAuthorizationMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto);

        Assert.False(siguienteInvocado);
        Assert.Equal(StatusCodes.Status401Unauthorized, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AutenticadoSinClaimDeRol_Devuelve403YNoLlamaASiguiente()
    {
        var usuario = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "alguien")], "TestAuth"));
        var contexto = CrearContexto(new RequiereRolAttribute(RolUsuario.Admin), usuario);
        var siguienteInvocado = false;
        var middleware = new RoleAuthorizationMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto);

        Assert.False(siguienteInvocado);
        Assert.Equal(StatusCodes.Status403Forbidden, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_RolAutenticadoNoPermitido_Devuelve403YNoLlamaASiguiente()
    {
        // Doctor intenta un endpoint que exige Admin o Lemy: debe rechazarse, no colarse.
        var usuario = UsuarioAutenticadoConRol(RolUsuario.Doctor);
        var contexto = CrearContexto(new RequiereRolAttribute(RolUsuario.Admin, RolUsuario.Lemy), usuario);
        var siguienteInvocado = false;
        var middleware = new RoleAuthorizationMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto);

        Assert.False(siguienteInvocado);
        Assert.Equal(StatusCodes.Status403Forbidden, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_RolAutenticadoPermitido_LlamaASiguiente()
    {
        var usuario = UsuarioAutenticadoConRol(RolUsuario.Lemy);
        var contexto = CrearContexto(new RequiereRolAttribute(RolUsuario.Admin, RolUsuario.Lemy), usuario);
        var siguienteInvocado = false;
        var middleware = new RoleAuthorizationMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto);

        Assert.True(siguienteInvocado);
        Assert.Equal(200, contexto.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ClaimDeRolConMayusculasDistintas_IgualSeReconoce()
    {
        // ignoreCase: true en Enum.TryParse — un claim "lemy" (minúscula) debe funcionar igual que "Lemy".
        var usuario = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "lemy")], "TestAuth"));
        var contexto = CrearContexto(new RequiereRolAttribute(RolUsuario.Lemy), usuario);
        var siguienteInvocado = false;
        var middleware = new RoleAuthorizationMiddleware(_ => { siguienteInvocado = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(contexto);

        Assert.True(siguienteInvocado);
    }
}
