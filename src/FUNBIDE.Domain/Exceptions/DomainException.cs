namespace FUNBIDE.Domain.Exceptions;

/// <summary>
/// Base type for exceptions that represent a violation of a business rule.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
