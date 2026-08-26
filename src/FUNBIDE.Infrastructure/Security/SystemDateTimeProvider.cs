using FUNBIDE.Application.Common.Interfaces;

namespace FUNBIDE.Infrastructure.Security;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    // Construida a mano (no TimeZoneInfo.FindSystemTimeZoneById) a propósito: buscar por
    // id del sistema depende de qué base de datos de zonas tenga el SO, y falla en
    // Windows con <InvariantGlobalization>true (ver FUNBIDE.API.csproj, activado para el
    // .exe autocontenido de la app de escritorio) — sin ICU, Windows no puede mapear el
    // id IANA "America/Santo_Domingo" a su propio "SA Western Standard Time". Santo
    // Domingo es UTC-4 fijo todo el año, sin horario de verano (ver también
    // AgendaPage.tsx en el frontend), así que un offset fijo es correcto siempre y no
    // depende de qué zonas tenga instaladas el SO — funciona igual en Windows o Linux,
    // con o sin globalización invariante.
    private static readonly TimeZoneInfo ZonaSantoDomingo = TimeZoneInfo.CreateCustomTimeZone(
        "America/Santo_Domingo", TimeSpan.FromHours(-4), "Santo Domingo", "Hora de Santo Domingo");

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public TimeZoneInfo ZonaHorariaClinica => ZonaSantoDomingo;
}
