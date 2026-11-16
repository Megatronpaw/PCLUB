using PClub.Application.Common;
using PClub.Domain.Entities;

namespace PClub.Application.Wallets;

/// <summary>Доступ к кошелькам и операциям.</summary>
public interface IWalletRepository
{
    /// <summary>Пользователь для изменения баланса — отслеживаемый.</summary>
    Task<User?> GetTrackedUserAsync(
        Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Добавляет запись операции.</summary>
    void AddTransaction(WalletTransaction transaction);

    /// <summary>Была ли оплачена бронь: старые бесплатные брони не возвращают деньги.</summary>
    Task<bool> HasBookingChargeAsync(Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Страница выписки, новые операции первыми.</summary>
    Task<PagedResult<WalletTransactionDto>> GetStatementAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сумма всех операций пользователя. Для сверки с балансом.
    /// </summary>
    Task<long> GetTransactionSumAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
