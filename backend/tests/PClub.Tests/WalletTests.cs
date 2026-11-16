using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PClub.Application.Abstractions;
using PClub.Application.Bookings;
using PClub.Application.Wallets;
using PClub.Domain;
using PClub.Domain.Entities;
using PClub.Domain.Enums;
using PClub.Domain.Exceptions;
using PClub.Infrastructure.Bookings;
using PClub.Infrastructure.Clubs;
using PClub.Infrastructure.Persistence;
using PClub.Infrastructure.Wallets;
using Xunit;

namespace PClub.Tests;

public sealed class WalletTests
{
    private static readonly DateTimeOffset Now = new(2030, 1, 1, 10, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(24, 2001, 2001)]
    [InlineData(25, 2001, 2001)]
    [InlineData(3, 2001, 1001)]
    [InlineData(2, 2000, 1000)]
    public void RefundPolicy(int hours, long price, long expected)
    {
        var booking = NewBooking(Now.AddHours(hours), price);
        Assert.Equal(0, booking.CalculateRefundCents(Now));
        booking.Cancel(Now);
        Assert.Equal(expected, booking.CalculateRefundCents(Now));
        booking.MarkRefunded(expected);
        Assert.Equal(expected, booking.RefundedCents);
        Assert.Throws<DomainValidationException>(() => booking.MarkRefunded(expected));
        Assert.Throws<ConflictException>(() => booking.Cancel(Now));
    }

    [Fact]
    public void TooLateCancellationLeavesBookingConfirmed()
    {
        var booking = NewBooking(Now.AddHours(1), 2000);
        Assert.Throws<ConflictException>(() => booking.Cancel(Now));
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void MoneyPreservesCentsAndRoundsHalfAwayFromZero()
    {
        Assert.Equal(1001, Money.ForDuration(2001, TimeSpan.FromMinutes(30)));
        Assert.Equal(3002, Money.ForDuration(2001, TimeSpan.FromMinutes(90)));
        Assert.Equal(9_007_199_254_740_993L,
            Money.ForDuration(9_007_199_254_740_993L, TimeSpan.FromHours(1)));
    }

    [Fact]
    public void WalletRejectsInvalidDebitsAndOverflowWithoutChangingBalance()
    {
        var user = new User("test@example.com", "Test");
        user.Credit(1000);
        var error = Assert.Throws<InsufficientFundsException>(() => user.Debit(1001));
        Assert.Equal(1, error.ShortfallCents);
        Assert.Equal(1000, user.BalanceCents);
        Assert.Throws<DomainValidationException>(() => user.Debit(0));
        Assert.Throws<DomainValidationException>(() => user.Credit(-1));
        user.Credit(long.MaxValue - 1000);
        Assert.Throws<OverflowException>(() => user.Credit(1));
        Assert.Equal(long.MaxValue, user.BalanceCents);
    }

    [Theory]
    [InlineData(500, false)]
    [InlineData(1000, true)]
    [InlineData(10000000, true)]
    [InlineData(10000001, false)]
    public void TopUpLimits(long amount, bool valid) => Assert.Equal(valid,
        new TopUpRequestValidator().Validate(new TopUpRequest(Guid.NewGuid(), amount)).IsValid);

    [Fact]
    public void RefundCanExceedTopUpLimit()
    {
        var user = new User("test@example.com", "Test");
        Assert.Equal(User.MaxTopUpCents + 1, user.Credit(User.MaxTopUpCents + 1));
    }

    [Fact]
    public void LedgerRejectsInvalidSignsAndMissingBooking()
    {
        Assert.Throws<DomainValidationException>(() => new WalletTransaction(Guid.NewGuid(),
            WalletTransactionType.BookingCharge, 100, 100, "Charge", Now, Guid.NewGuid()));
        Assert.Throws<DomainValidationException>(() => new WalletTransaction(Guid.NewGuid(),
            WalletTransactionType.BookingRefund, -100, 100, "Refund", Now, Guid.NewGuid()));
        Assert.Throws<DomainValidationException>(() => new WalletTransaction(Guid.NewGuid(),
            WalletTransactionType.BookingCharge, -100, 100, "Charge", Now));
    }

    private static Booking NewBooking(DateTimeOffset start, long price) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            start, start.AddHours(1), price, Now);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PostgreSqlMoneyFlowAndConcurrency(bool providerRetry)
    {
        var connection = Environment.GetEnvironmentVariable("PCLUB_TEST_POSTGRES");
        Assert.False(string.IsNullOrEmpty(connection),
            "Set PCLUB_TEST_POSTGRES to a dedicated empty PostgreSQL test database (see tests README).");
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connection,
                pg => { if (providerRetry) pg.EnableRetryOnFailure(3); })
            .UseSnakeCaseNamingConvention().Options;
        await using var setup = new AppDbContext(options);
        await setup.Database.MigrateAsync();
        var user = new User($"{Guid.NewGuid()}@example.com", "Wallet test");
        var club = new Club("Test", "Test", "Test", new TimeOnly(0), new TimeOnly(0));
        var zone = new Zone("Test", 2001);
        var seat = new Seat("Test");
        zone.AddSeat(seat);
        club.AddZone(zone);
        club.ChangeStatus(ClubStatus.Published, true);
        setup.AddRange(user, club);
        await setup.SaveChangesAsync();

        async Task<T> Run<T>(Func<WalletService, BookingService, AppDbContext, Task<T>> operation)
        {
            await using var db = new AppDbContext(options);
            var clock = new FixedClock();
            var uow = new UnitOfWork(db, NullLogger<UnitOfWork>.Instance);
            var wallets = new EfWalletRepository(db);
            return await operation(new WalletService(wallets, uow, clock),
                new BookingService(new EfBookingRepository(db), new EfClubRepository(db, clock),
                    uow, clock, wallets), db);
        }

        CreateBookingRequest Request(int hours) => new(user.Id, seat.Id, Now.AddHours(hours), Now.AddHours(hours + 1));
        await Assert.ThrowsAsync<InsufficientFundsException>(() => Run((w, b, db) => b.CreateAsync(Request(48))));
        Assert.Equal(0, await setup.Bookings.CountAsync(b => b.UserId == user.Id));
        await Run((w, b, db) => w.TopUpAsync(new(user.Id, 100000)));
        var full = await Run((w, b, db) => b.CreateAsync(Request(48)));
        await Assert.ThrowsAsync<ConflictException>(() => Run((w, b, db) => b.CreateAsync(Request(48))));
        var cancelled = await Run((w, b, db) => b.CancelAsync(full.Id, user.Id));
        Assert.Equal(2001, cancelled.RefundedCents);
        await Assert.ThrowsAsync<ConflictException>(() => Run((w, b, db) => b.CancelAsync(full.Id, user.Id)));
        var half = await Run((w, b, db) => b.CreateAsync(Request(3)));
        var refunds = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            try { await Run((w, b, db) => b.CancelAsync(half.Id, user.Id)); return true; }
            catch (ConflictException) { return false; }
        }));
        Assert.Single(refunds, x => x);
        Assert.Equal(1001, (await Run((w, b, db) => b.GetByIdAsync(half.Id))).RefundedCents);
        var late = await Run((w, b, db) => b.CreateAsync(Request(1)));
        await Assert.ThrowsAsync<ConflictException>(() => Run((w, b, db) => b.CancelAsync(late.Id, user.Id)));
        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Run((w, b, db) => w.TopUpAsync(new(user.Id, 1000)))));
        var balance = await Run((w, b, db) => w.GetAsync(user.Id));
        Assert.True(balance.IsConsistent);
        Assert.Equal(106999, balance.BalanceCents);
        Assert.Equal(16, (await Run((w, b, db) => w.GetStatementAsync(user.Id, 1, 50))).TotalCount);

        var legacy = new Booking(user.Id, club.Id, zone.Id, seat.Id, Now.AddHours(72), Now.AddHours(73), 2001, Now);
        setup.Add(legacy);
        await setup.SaveChangesAsync();
        Assert.Equal(0, (await Run((w, b, db) => b.CancelAsync(legacy.Id, user.Id))).RefundedCents);
        Assert.Equal(balance.BalanceCents, (await Run((w, b, db) => w.GetAsync(user.Id))).BalanceCents);
    }

    private sealed class FixedClock : IClock { public DateTimeOffset UtcNow => Now; }
}
