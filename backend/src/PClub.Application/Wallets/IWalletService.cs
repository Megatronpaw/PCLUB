using PClub.Application.Common;

namespace PClub.Application.Wallets;

/// <summary>Операции с кошельком.</summary>
public interface IWalletService
{
    /// <summary>Пополняет кошелёк.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">Пользователя нет.</exception>
    /// <exception cref="Domain.Exceptions.DomainValidationException">Сумма вне границ.</exception>
    Task<WalletDto> TopUpAsync(
        TopUpRequest request, CancellationToken cancellationToken = default);

    /// <summary>Состояние кошелька со сверкой.</summary>
    /// <exception cref="Domain.Exceptions.NotFoundException">Пользователя нет.</exception>
    Task<WalletDto> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Страница выписки.</summary>
    Task<PagedResult<WalletTransactionDto>> GetStatementAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
