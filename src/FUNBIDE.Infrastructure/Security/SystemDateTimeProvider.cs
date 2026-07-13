using FUNBIDE.Application.Common.Interfaces;

namespace FUNBIDE.Infrastructure.Security;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo ZonaSantoDomingo = TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public TimeZoneInfo ZonaHorariaClinica => ZonaSantoDomingo;
}
