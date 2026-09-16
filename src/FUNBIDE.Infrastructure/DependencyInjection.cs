using System.Net.Http.Headers;
using FUNBIDE.Application.Common.Interfaces;
using FUNBIDE.Domain.Interfaces;
using FUNBIDE.Infrastructure.BackgroundServices;
using FUNBIDE.Infrastructure.Caching;
using FUNBIDE.Infrastructure.Files;
using FUNBIDE.Infrastructure.Health;
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
                npgsql => npgsql
                    .MigrationsHistoryTable("__ef_migrations_history", "funbide")
                    // Sin esto, un query colgado (Supabase caído, o una consulta que se
                    // olvidó un índice) deja la petición esperando indefinidamente en vez
                    // de fallar con un error legible -- 30s es generoso para cualquier
                    // query normal de la app, pero corta antes de que alguien piense que
                    // la app "se congeló". Ver ExceptionHandlingMiddleware, que traduce el
                    // NpgsqlException resultante a un mensaje claro en vez del 500 genérico.
                    .CommandTimeout(30)));

        services.AddScoped<ICitaRepository, CitaRepository>();
        services.AddScoped<IHistorialClinicoRepository, HistorialClinicoRepository>();
        services.AddScoped<IInventarioRepository, InventarioRepository>();
        services.AddScoped<IAuditoriaLogRepository, AuditoriaLogRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IMovimientoFinancieroRepository, MovimientoFinancieroRepository>();
        services.AddScoped<IResumenDiarioRepository, ResumenDiarioRepository>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<IPacienteRepository, PacienteRepository>();
        services.AddScoped<ITurnoCajaRepository, TurnoCajaRepository>();
        services.AddScoped<ICobroRepository, CobroRepository>();
        services.AddScoped<ISeguroMedicoRepository, SeguroMedicoRepository>();
        services.AddScoped<ITarifarioProcedimientoRepository, TarifarioProcedimientoRepository>();
        services.AddScoped<IServicioRepository, ServicioRepository>();
        services.AddScoped<IDonacionRepository, DonacionRepository>();
        services.AddScoped<IConfiguracionSistemaRepository, ConfiguracionSistemaRepository>();
        services.AddScoped<ISesionActivaRepository, SesionActivaRepository>();
        services.AddScoped<IPermisoUsuarioRepository, PermisoUsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMemoryCache();
        services.AddSingleton<IConfiguracionSistemaCache, ConfiguracionSistemaCache>();

        // "database" es el nombre que después lee /health en Program.cs para armar el
        // detalle de la respuesta -- si se renombra acá, hay que renombrarlo ahí también.
        services.AddHealthChecks().AddCheck<SupabaseHealthCheck>("database");

        services.AddScoped<IAuditoriaLogService, AuditoriaLogService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermisoResolverService, PermisoResolverService>();
        services.AddScoped<IEstadoBaseDeDatosService, EstadoBaseDeDatosService>();
        services.AddScoped<IEstadoBackupService, EstadoBackupService>();
        services.AddScoped<IEspacioDiscoService, EspacioDiscoService>();
        // Singleton, no Scoped: BackupEjecutorService (el único consumidor) es Singleton
        // -- un servicio Scoped ahí sería un "captive dependency" que la validación de DI
        // de ASP.NET Core rechaza al construir el host. AlertaAdminLogService no tiene
        // estado por-request (solo envuelve un ILogger), así que ser singleton es seguro.
        services.AddSingleton<IAlertaAdminService, AlertaAdminLogService>();
        services.AddScoped<IExcelLectorService, ExcelLectorService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddFunbideCors(configuration);

        // Auth:Provider elige entre Supabase (nube, producción) y Local (JWT emitido
        // por esta misma API sin red, para la copia offline/USB de demo). Ver
        // AuthOptions/JwtAuthenticationExtensions/LocalAuthAdminService.
        var proveedorAuth = configuration.GetSection(AuthOptions.SeccionConfiguracion).Get<AuthOptions>()?.Provider
            ?? ProveedorAutenticacion.Supabase;

        if (proveedorAuth == ProveedorAutenticacion.Local)
        {
            services.AddLocalJwtAuthentication(configuration);
            services.AddScoped<ISupabaseAdminService, LocalAuthAdminService>();
            services.AddScoped<IAutenticacionLocalService, AutenticacionLocalService>();
            services.AddScoped<ISembradorUsuarioInicialLocalService, SembradorUsuarioInicialLocalService>();

            services
                .AddOptions<LocalStorageOptions>()
                .Bind(configuration.GetSection(LocalStorageOptions.SeccionConfiguracion))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            services.AddScoped<ISupabaseStorageService, LocalDiskStorageService>();
            services.AddScoped<IArchivoLocalService, ArchivoLocalService>();
        }
        else
        {
            services.AddSupabaseJwtAuthentication(configuration);
            services.AddScoped<IAutenticacionLocalService, AutenticacionLocalNoDisponibleService>();
            services.AddScoped<IArchivoLocalService, ArchivoLocalNoDisponibleService>();

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
        }

        // Backup:Habilitado permite apagar el respaldo automático por pg_dump en la
        // copia offline/USB (no tiene sentido ni pg_dump.exe ni destino de backup ahí).
        var backupHabilitado = configuration.GetValue("Backup:Habilitado", true);
        if (backupHabilitado)
        {
            services
                .AddOptions<BackupOptions>()
                .Bind(configuration.GetSection(BackupOptions.SeccionConfiguracion))
                .ValidateOnStart();
            services.AddSingleton<AesBackupEncryptor>();
            services.AddSingleton<IBackupEjecutorService, BackupEjecutorService>();
            services.AddHostedService<DatabaseBackupHostedService>();
        }
        else
        {
            services.AddSingleton<IBackupEjecutorService, BackupEjecutorNoDisponibleService>();
        }

        return services;
    }

    private static void ConfigurarAutenticacionServiceRole(HttpClient client, string serviceRoleKey)
    {
        client.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", serviceRoleKey);
    }
}
