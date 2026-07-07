using FUNBIDE.API.Extensions;
using FUNBIDE.Infrastructure;
using FUNBIDE.Infrastructure.Logging;
using FUNBIDE.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

var app = builder.Build();

// Despliegue de un solo contenedor sin paso de migración separado: aplica al
// arrancar cualquier migración pendiente contra la base de datos, para que el
// esquema nunca quede desincronizado con el código desplegado.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FunbideDbContext>();
    await dbContext.Database.MigrateAsync();
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
