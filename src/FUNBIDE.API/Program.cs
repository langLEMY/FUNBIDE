using FUNBIDE.API.Extensions;
using FUNBIDE.Infrastructure;
using FUNBIDE.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseFunbidePipeline();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { estado = "ok" })).AllowAnonymous();

app.Run();
