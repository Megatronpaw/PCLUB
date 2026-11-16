namespace PClub.Domain.Exceptions;

/// <summary>Личность не подтверждена.</summary>
public sealed class UnauthorizedException : DomainException
{
    /// <summary>Создаёт исключение.</summary>
    public UnauthorizedException(string message) : base(message)
    {
    }
}
