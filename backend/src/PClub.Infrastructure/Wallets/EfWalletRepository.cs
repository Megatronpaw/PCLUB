using Microsoft.EntityFrameworkCore;
using PClub.Application.Common;
using PClub.Application.Wallets;
using PClub.Domain.Entities;
using PClub.Domain.Enums;
using PClub.Infrastructure.Persistence;

namespace PClub.Infrastructure.Wallets;

/// <summary>Доступ к кошелькам через EF Core.</summary>
public sealed class EfWalletRepository : IWalletRepository
{
    private readonly AppDbContext _db;

    /// <summary>Создаёт репозиторий.</summary>
    public EfWalletRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<User?> GetTrackedUserAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> HasBookingChargeAsync(Guid bookingId, CancellationToken cancellationToken = default) =>
        _db.WalletTransactions.AnyAsync(t => t.BookingId == bookingId
            && t.Type == WalletTransactionType.BookingCharge, cancellationToken);

    /// <inheritdoc />
    public void AddTransaction(WalletTransaction transaction) =>
        _db.WalletTransactions.Add(transaction);

    /// <inheritdoc />
    public async Task<PagedResult<WalletTransactionDto>> GetStatementAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _db.WalletTransactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new WalletTransactionDto(
                t.Id,
                t.Type,
                t.AmountCents,
                t.BalanceAfterCents,
                t.Description,
                t.BookingId,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<WalletTransactionDto>(items, page, pageSize, totalCount);
    }

    /// <inheritdoc />
    public async Task<long> GetTransactionSumAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await _db.WalletTransactions
            .Where(t => t.UserId == userId)
            .SumAsync(t => t.AmountCents, cancellationToken);
}
