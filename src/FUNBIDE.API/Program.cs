using FUNBIDE.API.Extensions;
using FUNBIDE.Infrastructure;
using FUNBIDE.Infrastructure.Logging;
using FUNBIDE.Infrastructure.Persistence;
using FUNBIDE.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
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
using (var scope = app.Services.CreateScope())
{
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

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseFunbidePipeline();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { estado = "ok" })).AllowAnonymous();
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();
