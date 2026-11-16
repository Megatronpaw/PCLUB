namespace PClub.Domain.Exceptions;

/// <summary>Не хватает средств на кошельке.</summary>
public sealed class InsufficientFundsException : DomainException
{
    /// <summary>Создаёт исключение.</summary>
    /// <param name="balanceCents">Сколько есть.</param>
    /// <param name="requiredCents">Сколько нужно.</param>
    public InsufficientFundsException(long balanceCents, long requiredCents)
        : base($"Недостаточно средств: на балансе {balanceCents / 100m:F2} руб., " +
               $"требуется {requiredCents / 100m:F2} руб.")
    {
        BalanceCents = balanceCents;
        RequiredCents = requiredCents;
    }

    /// <summary>Сколько было на балансе.</summary>
    public long BalanceCents { get; }

    /// <summary>Сколько требовалось.</summary>
    public long RequiredCents { get; }

    /// <summary>Сколько не хватило.</summary>
    public long ShortfallCents => RequiredCents - BalanceCents;
}
