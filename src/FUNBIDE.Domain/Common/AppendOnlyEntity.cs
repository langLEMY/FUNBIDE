namespace FUNBIDE.Domain.Common;

/// <summary>
/// Marker base type for entities that represent immutable, append-only records
/// (clinical history entries, audit logs). These must never be updated or deleted
/// once persisted; enforcement happens at the API and repository boundaries.
/// </summary>
public abstract class AppendOnlyEntity : Entity
{
    public DateTimeOffset RegistradoEn { get; protected init; } = DateTimeOffset.UtcNow;
}
