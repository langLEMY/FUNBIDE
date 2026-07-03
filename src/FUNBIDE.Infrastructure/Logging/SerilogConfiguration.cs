using Microsoft.Extensions.Configuration;
using NpgsqlTypes;
using Serilog;
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
            .WriteTo.PostgreSQL(
                connectionString: connectionString,
                tableName: "auditoria_logs",
                schemaName: "funbide",
                columnOptions: columnas,
                needAutoCreateTable: false,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
    }
}
