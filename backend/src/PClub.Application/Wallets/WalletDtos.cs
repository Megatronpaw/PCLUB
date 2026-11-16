using PClub.Domain.Enums;

namespace PClub.Application.Wallets;

/// <summary>Запрос пополнения.</summary>
/// <param name="UserId">Кому. До главы 11 приходит от клиента.</param>
/// <param name="AmountCents">Сколько копеек.</param>
public sealed record TopUpRequest(Guid UserId, long AmountCents);

/// <summary>Состояние кошелька.</summary>
/// <param name="UserId">Владелец.</param>
/// <param name="BalanceCents">Баланс в копейках.</param>
/// <param name="TransactionSumCents">Сумма всех операций — для сверки.</param>
/// <param name="IsConsistent">Сходится ли баланс с суммой операций.</param>
public sealed record WalletDto(
    Guid UserId,
    long BalanceCents,
    long TransactionSumCents,
    bool IsConsistent);

/// <summary>Операция в выписке.</summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Type">Вид.</param>
/// <param name="AmountCents">Сумма со знаком.</param>
/// <param name="BalanceAfterCents">Баланс после.</param>
/// <param name="Description">Пояснение.</param>
/// <param name="BookingId">Связанная бронь, если есть.</param>
/// <param name="CreatedAt">Когда.</param>
public sealed record WalletTransactionDto(
    Guid Id,
    WalletTransactionType Type,
    long AmountCents,
    long BalanceAfterCents,
    string Description,
    Guid? BookingId,
    DateTimeOffset CreatedAt);
