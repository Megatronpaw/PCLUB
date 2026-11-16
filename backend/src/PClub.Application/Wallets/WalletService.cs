using PClub.Application.Abstractions;
using PClub.Application.Common;
using PClub.Domain.Entities;
using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Application.Wallets;

/// <summary>Операции с кошельком.</summary>
public sealed class WalletService : IWalletService
{
    private readonly IWalletRepository _wallets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    /// <summary>Создаёт сервис.</summary>
    public WalletService(
        IWalletRepository wallets, IUnitOfWork unitOfWork, IClock clock)
    {
        _wallets = wallets;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    /// <inheritdoc />
    public Task<WalletDto> TopUpAsync(
        TopUpRequest request, CancellationToken cancellationToken = default) =>
        _unitOfWork.ExecuteInTransactionAsync(
            token => TopUpCoreAsync(request, token), cancellationToken);

    /// <inheritdoc />
    public Task<WalletDto> GetAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _unitOfWork.ExecuteInTransactionAsync(token => GetCoreAsync(userId, token), cancellationToken);

    private async Task<WalletDto> GetCoreAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _wallets.GetTrackedUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User", userId);

        var sum = await _wallets.GetTransactionSumAsync(userId, cancellationToken);

        return new WalletDto(
            user.Id,
            user.BalanceCents,
            sum,
            IsConsistent: user.BalanceCents == sum);
    }

    /// <inheritdoc />
    public Task<PagedResult<WalletTransactionDto>> GetStatementAsync(
        Guid userId, int page, int pageSize,
        CancellationToken cancellationToken = default) =>
        _wallets.GetStatementAsync(userId, page, pageSize, cancellationToken);

    /// <summary>Пополнение внутри транзакции.</summary>
    private async Task<WalletDto> TopUpCoreAsync(
        TopUpRequest request, CancellationToken cancellationToken)
    {
        var user = await _wallets.GetTrackedUserAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);
        var validation = new TopUpRequestValidator().Validate(request);
        if (!validation.IsValid)
            throw new DomainValidationException(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        var balanceAfter = user.Credit(request.AmountCents);

        _wallets.AddTransaction(new WalletTransaction(
            userId: user.Id,
            type: WalletTransactionType.TopUp,
            amountCents: request.AmountCents,
            balanceAfterCents: balanceAfter,
            description: $"Пополнение на {request.AmountCents / 100m:F2} руб.",
            now: _clock.UtcNow));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sum = await _wallets.GetTransactionSumAsync(user.Id, cancellationToken);

        return new WalletDto(user.Id, balanceAfter, sum, balanceAfter == sum);
    }
}
