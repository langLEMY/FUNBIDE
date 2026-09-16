using Microsoft.Extensions.Configuration;
using NpgsqlTypes;
using Serilog;
using Serilog.Filters;
using Serilog.Sinks.PostgreSQL;

namespace FUNBIDE.Infrastructure.Logging;

/// <summary>
/// Centraliza el mapeo entre las propiedades estructuradas que emite
/// <c>RequestAuditLoggingMiddleware</c> (Usuario, Accion, Recurso, CodigoRespuestaHttp)
/// y las columnas JSONB de "funbide.auditoria_logs", para que cada request quede
/// escrita como un registro de auditoría sin código duplicado entre logging y dominio.
/// </summary>
public static class SerilogConfiguration
{
    public static LoggerConfiguration ConfigurarFunbideLogging(
        this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FunbideDatabase")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'FunbideDatabase'.");

        var columnas = new Dictionary<string, ColumnWriterBase>
        {
            { "usuario_id", new SinglePropertyColumnWriter("UsuarioId", PropertyWriteMethod.Raw, NpgsqlDbType.Uuid) },
            { "accion", new SinglePropertyColumnWriter("Accion", PropertyWriteMethod.ToString, NpgsqlDbType.Text) },
            { "recurso", new SinglePropertyColumnWriter("Recurso", PropertyWriteMethod.ToString, NpgsqlDbType.Text) },
            { "codigo_respuesta_http", new SinglePropertyColumnWriter("CodigoRespuestaHttp", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "detalle", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
            { "registrado_en", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) }
        };

        return loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.Console()
            // Filter.ByExcluding aquí evita que eventos marcados con "ExcluirDeAuditoria"
            // (p. ej. la contraseña temporal del admin inicial, que sí debe verse en
            // "docker compose logs" pero no debe quedar persistida indefinidamente en
            // la tabla de auditoría / backups de la base) lleguen al sink de Postgres.
            //
            // También se excluye RequestAuditLoggingMiddleware por su SourceContext: ese
            // middleware loguea un evento con las MISMAS propiedades (Accion/Recurso/
            // CodigoRespuestaHttp) por CADA petición HTTP -- sin este filtro, cada GET, cada
            // polling del sidebar (cada 20s) terminaba como una fila más en
            // "auditoria_logs" junto a los eventos de negocio reales (cobros.registrar,
            // pacientes.crear, etc.), ahogándolos en ruido técnico. Ese log técnico por
            // request sigue existiendo (ver items de observabilidad) -- solo deja de
            // mezclarse con la auditoría de negocio que ve Lemy en "Actividad".
            .WriteTo.Logger(sublogger => sublogger
                .Filter.ByExcluding(Matching.WithProperty("ExcluirDeAuditoria"))
                .Filter.ByExcluding(Matching.FromSource("FUNBIDE.API.Middleware.RequestAuditLoggingMiddleware"))
                .WriteTo.PostgreSQL(
                    connectionString: connectionString,
                    tableName: "auditoria_logs",
                    schemaName: "funbide",
                    columnOptions: columnas,
                    needAutoCreateTable: false,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information));
    }
}
