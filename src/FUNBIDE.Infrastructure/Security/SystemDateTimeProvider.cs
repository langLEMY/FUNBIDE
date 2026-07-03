using FUNBIDE.Application.Common.Interfaces;

namespace FUNBIDE.Infrastructure.Security;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
