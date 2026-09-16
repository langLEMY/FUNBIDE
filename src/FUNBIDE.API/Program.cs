using FUNBIDE.API.Extensions;
using FUNBIDE.Infrastructure;
using FUNBIDE.Infrastructure.Logging;
using FUNBIDE.Infrastructure.Persistence;
using FUNBIDE.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ConfigurarFunbideLogging(context.Configuration));

builder.Services
    .AddControllers(options => options.Filters.Add<ValidationActionFilter>())
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFunbideInfrastructure(builder.Configuration);
builder.Services.AddFunbideUseCases();
builder.Services.AddFunbideValidators();

// Requisito base: todo endpoint exige un usuario autenticado por defecto.
// El refinamiento por rol lo aplica RoleAuthorizationMiddleware con [RequiereRol].
builder.Services.Configure<AuthorizationOptions>(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// Nginx (deploy/nginx/nginx.conf) reenvía X-Forwarded-For/-Proto desde un contenedor
// vecino, no desde loopback: se limpian KnownNetworks/KnownProxies para que ASP.NET
// confíe en ese salto y HttpContext.Connection.RemoteIpAddress refleje la IP real del
// cliente (usada por el registro de intentos de inicio de sesión).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Frena fuerza bruta contra POST /api/auth/login (modo Auth:Provider=Local: es el único
// endpoint anónimo que verifica una contraseña). Particionado por IP (RateLimitPartition):
// AddFixedWindowLimiter a secas crea UN cupo global compartido por todos los clientes, así
// que 10 requests desde cualquier IP agotaban el cupo y bloqueaban el login de todos los
// demás (DoS trivial no autenticado). Con partición, cada IP tiene su propio cupo de 10/min.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login-local", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    // /api/auth/eventos-login es anónimo (registra tanto éxitos como fallos de un login
    // que ocurre fuera de esta API, ver AuthController) y confía en correo/exitoso que
    // manda el cliente sin verificarlos contra nada — sin límite, cualquiera podía inflar
    // la bitácora de auditoría con eventos falsos para cualquier correo, en bucle. Cupo
    // más generoso que login-local porque acá sí se espera tráfico legítimo repetido
    // (varios intentos fallidos reales de un usuario que escribe mal su contraseña).
    options.AddPolicy("eventos-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

var app = builder.Build();

// Despliegue de un solo contenedor sin paso de migración separado: aplica al
// arrancar cualquier migración pendiente contra la base de datos, para que el
// esquema nunca quede desincronizado con el código desplegado.
//
// Database:AplicarMigracionesAlIniciar (default true) queda en false en la copia de
// escritorio (Auth:Provider=Local apuntando a la misma base compartida de producción,
// ver publicar-offline.ps1 e instalacion/FUNBIDE.iss): ahí cada instalación de personal
// abre su propio proceso contra la MISMA base, así que dejarlas migrar sería una
// carrera entre varias copias corriendo DDL a la vez — y el rol acotado que usa esa
// copia (funbide_app, sin privilegios de esquema) ni siquiera podría aplicarlas. El
// esquema real ya lo migra el único despliegue de servidor (Railway), que sigue usando
// el rol con privilegios completos.
if (builder.Configuration.GetValue("Database:AplicarMigracionesAlIniciar", true))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FunbideDbContext>();
    await dbContext.Database.MigrateAsync();

    // Solo registrado en modo Auth:Provider=Local (ver DependencyInjection). En modo
    // Supabase no hay nada que sembrar: el primer usuario se crea desde el dashboard.
    var sembrador = scope.ServiceProvider.GetService<ISembradorUsuarioInicialLocalService>();
    if (sembrador is not null)
    {
        await sembrador.SembrarSiNecesarioAsync(CancellationToken.None);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// index.html nunca se debe cachear: los archivos en /assets ya llevan un hash de
// contenido en el nombre (AseguradorasPage-<hash>.js), así que un build nuevo con
// contenido distinto genera un nombre distinto — cachearlos "para siempre" es seguro y
// deseado. Pero index.html SIEMPRE se llama igual, y es el único lugar donde viven esos
// nombres hasheados; si el navegador (WebView2, con un perfil persistente entre
// arranques — ver launcher/FormPrincipal.cs) lo cachea, cada nueva versión del instalador
// queda invisible para siempre aunque los archivos en disco ya estén actualizados. Visto
// en producción: un cambio de UI no llegaba a verse tras reinstalar, aun con los archivos
// nuevos correctamente presentes en wwwroot.
var opcionesArchivosEstaticos = new StaticFileOptions
{
    OnPrepareResponse = contexto =>
    {
        if (contexto.File.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
        {
            contexto.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            contexto.Context.Response.Headers.Pragma = "no-cache";
            contexto.Context.Response.Headers.Expires = "0";
        }
    },
};

app.UseDefaultFiles();
app.UseStaticFiles(opcionesArchivosEstaticos);

app.UseHttpsRedirection();
app.UseFunbidePipeline();

app.MapControllers();

// Antes devolvía {estado:"ok"} siempre, sin verificar nada -- ahora corre
// SupabaseHealthCheck (Database.CanConnectAsync real) y responde 503 si la base no es
// alcanzable, para que tanto el healthcheck de Docker (docker-compose.yml) como quien
// mire /health a mano vean el estado real, no solo "el proceso sigue vivo".
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var cuerpo = JsonSerializer.Serialize(new
        {
            estado = report.Status == HealthStatus.Healthy ? "ok" : "degradado",
            baseDeDatos = report.Entries.TryGetValue("database", out var entrada)
                ? entrada.Status == HealthStatus.Healthy ? "ok" : "no disponible"
                : "desconocido",
        });
        await context.Response.WriteAsync(cuerpo);
    }
}).AllowAnonymous();

app.MapFallbackToFile("index.html", opcionesArchivosEstaticos).AllowAnonymous();

app.Run();
