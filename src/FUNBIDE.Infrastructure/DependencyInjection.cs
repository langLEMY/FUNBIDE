using System.Net.Http.Headers;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Infrastructure.BackgroundServices;
using FUNBIDE.Infrastructure.Logging;
using FUNBIDE.Infrastructure.Persistence;
using FUNBIDE.Infrastructure.Persistence.Repositories;
using FUNBIDE.Infrastructure.Security;
using FUNBIDE.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FUNBIDE.Infrastructure;

/// <summary>
/// Único punto de composición de la capa Infrastructure. La API solo llama a
/// <see cref="AddFunbideInfrastructure"/>; no conoce Npgsql, EF Core ni Serilog.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddFunbideInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FunbideDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("FunbideDatabase"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "funbide")));

        services.AddScoped<ICitaRepository, CitaRepository>();
        services.AddScoped<IHistorialClinicoRepository, HistorialClinicoRepository>();
        services.AddScoped<IInventarioRepository, InventarioRepository>();
        services.AddScoped<IAuditoriaLogRepository, AuditoriaLogRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IMovimientoFinancieroRepository, MovimientoFinancieroRepository>();
        services.AddScoped<IResumenDiarioRepository, ResumenDiarioRepository>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuditoriaLogService, AuditoriaLogService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEstadoBaseDeDatosService, EstadoBaseDeDatosService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddSupabaseJwtAuthentication(configuration);
        services.AddFunbideCors(configuration);

        services
            .AddOptions<SupabaseAdminOptions>()
            .Bind(configuration.GetSection(SupabaseAdminOptions.SeccionConfiguracion))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<SupabaseStorageOptions>()
            .Bind(configuration.GetSection(SupabaseStorageOptions.SeccionConfiguracion));

        services.AddHttpClient<ISupabaseAdminService, SupabaseAdminService>((sp, client) =>
        {
            var opciones = sp.GetRequiredService<IOptions<SupabaseAdminOptions>>().Value;
            client.BaseAddress = new Uri($"{opciones.ProjectUrl.TrimEnd('/')}/auth/v1/admin/");
            ConfigurarAutenticacionServiceRole(client, opciones.ServiceRoleKey);
        });

        services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>((sp, client) =>
        {
            var opciones = sp.GetRequiredService<IOptions<SupabaseAdminOptions>>().Value;
            client.BaseAddress = new Uri($"{opciones.ProjectUrl.TrimEnd('/')}/storage/v1/");
            ConfigurarAutenticacionServiceRole(client, opciones.ServiceRoleKey);
        });

        services
            .AddOptions<BackupOptions>()
            .Bind(configuration.GetSection(BackupOptions.SeccionConfiguracion))
            .ValidateOnStart();
        services.AddSingleton<AesBackupEncryptor>();
        services.AddHostedService<DatabaseBackupHostedService>();

        return services;
    }

    private static void ConfigurarAutenticacionServiceRole(HttpClient client, string serviceRoleKey)
    {
        client.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceRoleKey);
    }
}
