namespace FUNBIDE.Infrastructure.BackgroundServices;

/// <summary>
/// Opciones del backup automático. Se enlazan desde la sección "Backup" de la
/// configuración; <see cref="AesKeyBase64"/> debe provenir de un secreto (variable
/// de entorno o vault), nunca de appsettings.json en texto plano en producción.
/// </summary>
public sealed class BackupOptions
{
    public const string SeccionConfiguracion = "Backup";

    public required string PgHost { get; init; }
    public int PgPort { get; init; } = 5432;
    public required string PgDatabase { get; init; }
    public required string PgUser { get; init; }
    public required string PgPassword { get; init; }

    /// <summary>Ruta al ejecutable pg_dump. En el contenedor de despliegue es "pg_dump".</summary>
    public string PgDumpPath { get; init; } = "pg_dump";

    public required string DirectorioDestino { get; init; }

    /// <summary>Clave AES-256 codificada en Base64 (32 bytes una vez decodificada).</summary>
    public required string AesKeyBase64 { get; init; }

    public TimeSpan Intervalo { get; init; } = TimeSpan.FromHours(24);

    public int BackupsARetener { get; init; } = 14;
}
