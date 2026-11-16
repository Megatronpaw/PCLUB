using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Domain.Entities;

/// <summary>
/// Одна операция по кошельку. Запись только добавляется — никогда
/// не изменяется и не удаляется.
/// </summary>
/// <remarks>
/// Отсюда и отсутствие методов изменения: у класса нет ни одного способа
/// поменять себя после создания. Это не забывчивость, а устройство:
/// ошибка исправляется обратной операцией, а не правкой старой записи.
/// </remarks>
public sealed class WalletTransaction
{
    /// <summary>Максимальная длина описания.</summary>
    public const int MaxDescriptionLength = 256;

    /// <summary>Создаёт запись операции.</summary>
    /// <param name="userId">Чей кошелёк.</param>
    /// <param name="type">Вид операции.</param>
    /// <param name="amountCents">
    /// Сумма со знаком: положительная — приход, отрицательная — расход.
    /// </param>
    /// <param name="balanceAfterCents">Баланс после операции.</param>
    /// <param name="description">Пояснение для выписки.</param>
    /// <param name="now">Текущий момент.</param>
    /// <param name="bookingId">Бронь, если операция связана с ней.</param>
    public WalletTransaction(
        Guid userId,
        WalletTransactionType type,
        long amountCents,
        long balanceAfterCents,
        string description,
        DateTimeOffset now,
        Guid? bookingId = null)
    {
        if (!Enum.IsDefined(type))
            throw new DomainValidationException("Неизвестный тип операции.");

        if (type != WalletTransactionType.TopUp && (bookingId is null || bookingId == Guid.Empty))
            throw new DomainValidationException("Для операции по брони нужна бронь.");

        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("Владелец операции обязателен.");
        }
        if (amountCents == 0)
        {
            throw new DomainValidationException("Сумма операции не может быть нулевой.");
        }

        if (balanceAfterCents < 0)
        {
            throw new DomainValidationException("Баланс не может стать отрицательным.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException("Описание операции обязательно.");
        }
        var expectedPositive = type is
            WalletTransactionType.TopUp or WalletTransactionType.BookingRefund;

        if (expectedPositive && amountCents < 0)
        {
            throw new DomainValidationException(
                $"Операция {type} должна иметь положительную сумму.");
        }

        if (type == WalletTransactionType.BookingCharge && amountCents > 0)
        {
            throw new DomainValidationException(
                "Списание за бронь должно иметь отрицательную сумму.");
        }

        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        AmountCents = amountCents;
        BalanceAfterCents = balanceAfterCents;
        Description = description.Trim()[
            ..Math.Min(description.Trim().Length, MaxDescriptionLength)];
        BookingId = bookingId;
        CreatedAt = now;
    }

    /// <summary>Для EF Core.</summary>
    private WalletTransaction() => Description = null!;

    /// <summary>Идентификатор.</summary>
    public Guid Id { get; private set; }

    /// <summary>Чей кошелёк.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Вид операции.</summary>
    public WalletTransactionType Type { get; private set; }

    /// <summary>Сумма со знаком, копейки.</summary>
    public long AmountCents { get; private set; }

    /// <summary>Баланс после операции, копейки.</summary>
    public long BalanceAfterCents { get; private set; }

    /// <summary>Пояснение для выписки.</summary>
    public string Description { get; private set; }

    /// <summary>Связанная бронь, если есть.</summary>
    public Guid? BookingId { get; private set; }

    /// <summary>Когда произошла.</summary>
    public DateTimeOffset CreatedAt { get; private set; }
}
