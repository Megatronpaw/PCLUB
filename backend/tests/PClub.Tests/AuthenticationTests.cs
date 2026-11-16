using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PClub.Application.Abstractions;
using PClub.Application.Auth;
using PClub.Domain.Entities;
using PClub.Domain.Enums;
using PClub.Domain.Exceptions;
using PClub.Infrastructure.Auth;
using PClub.Infrastructure.Persistence;
using PClub.Infrastructure.Security;
using PClub.Infrastructure.Time;
using Xunit;

namespace PClub.Tests;

[Collection("PostgreSQL")]
public sealed class AuthenticationTests
{
    [Fact]
    public void HashesAreSaltedAndVerified()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var first = hasher.Hash("Test1234!");
        Assert.NotEqual(first, hasher.Hash("Test1234!"));
        Assert.True(hasher.Verify("Test1234!", first));
        Assert.False(hasher.Verify("wrong", first));
        Assert.StartsWith("pbkdf2-sha256$600000$", first);
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad")]
    [InlineData("pbkdf2-sha256$600000$$")]
    [InlineData("pbkdf2-sha256$2147483647$AA==$AA==")]
    [InlineData("pbkdf2-sha256$1$not-base64$AA==")]
    public void InvalidStoredHashReturnsFalse(string hash) =>
        Assert.False(new Pbkdf2PasswordHasher().Verify("Test1234!", hash));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("password")]
    public void InvalidRegistrationIsRejectedWithoutThrowing(string? password) =>
        Assert.False(new RegisterRequestValidator().Validate(new RegisterRequest("a@b.com", password!, "Test")).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void MissingOrShortSigningKeyFailsAtStartup(string key) =>
        Assert.Throws<InvalidOperationException>(() => new JwtOptions { SigningKey = key }.Validate());

    [Fact]
    public void ExpiredRefreshCannotBeUsed()
    {
        var now = DateTimeOffset.UtcNow;
        var token = new RefreshToken(Guid.NewGuid(), "hash", Guid.NewGuid(), now);
        Assert.True(token.IsActive(now));
        Assert.False(token.IsActive(now + RefreshToken.Lifetime));
        Assert.Throws<ConflictException>(() => token.MarkUsed(now + RefreshToken.Lifetime));
    }

    [Fact]
    public async Task PostgreSqlRefreshRaceRevokesOnlyItsFamilyAndLogoutIsIdempotent()
    {
        var connection = Environment.GetEnvironmentVariable("PCLUB_TEST_POSTGRES");
        Assert.False(string.IsNullOrWhiteSpace(connection), "Set PCLUB_TEST_POSTGRES to the dedicated test database.");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connection, pg => pg.EnableRetryOnFailure(3)).UseSnakeCaseNamingConvention().Options;
        await using var setup = new AppDbContext(options);
        await setup.Database.MigrateAsync();
        var clock = new SystemClock();
        var tokens = new JwtTokenService(Options.Create(new JwtOptions
        {
            SigningKey = "test-only-key-not-used-outside-automated-tests-1234567890"
        }), clock);
        var hasher = new Pbkdf2PasswordHasher();
        async Task<T> Run<T>(Func<AuthService, Task<T>> operation)
        {
            await using var db = new AppDbContext(options);
            return await operation(new AuthService(new EfUserRepository(db), new EfRefreshTokenRepository(db),
                hasher, tokens, new UnitOfWork(db, NullLogger<UnitOfWork>.Instance), clock, new AnonymousUser()));
        }
        var email = $"{Guid.NewGuid()}@example.com";
        var registered = await Run(a => a.RegisterAsync(new(email, "Test1234!", "Test")));
        Assert.Equal(UserRole.User, registered.Role);
        var access = new JwtSecurityTokenHandler().ReadJwtToken(registered.AccessToken);
        Assert.Equal(registered.UserId.ToString(), access.Subject);
        Assert.InRange((access.ValidTo - access.ValidFrom).TotalMinutes, 14.99, 15.01);
        var storedHash = tokens.HashRefreshToken(registered.RefreshToken);
        Assert.True(await setup.RefreshTokens.AnyAsync(t => t.TokenHash == storedHash));
        Assert.False(await setup.RefreshTokens.AnyAsync(t => t.TokenHash == registered.RefreshToken));

        var independent = await Run(a => a.LoginAsync(new(email.ToUpperInvariant(), "Test1234!")));
        var rotated = await Run(a => a.RefreshAsync(new(registered.RefreshToken)));
        await Assert.ThrowsAsync<UnauthorizedException>(() => Run(a => a.RefreshAsync(new(registered.RefreshToken))));
        await Assert.ThrowsAsync<UnauthorizedException>(() => Run(a => a.RefreshAsync(new(rotated.RefreshToken))));
        var valid = await Run(a => a.RefreshAsync(new(independent.RefreshToken)));

        var race = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            try { return await Run(a => a.RefreshAsync(new(valid.RefreshToken))); }
            catch (UnauthorizedException) { return null; }
        }));
        var winner = Assert.Single(race, r => r is not null)!;
        await Assert.ThrowsAsync<UnauthorizedException>(() => Run(a => a.RefreshAsync(new(winner.RefreshToken))));

        var session = await Run(a => a.LoginAsync(new(email, "Test1234!")));
        for (var i = 0; i < 2; i++)
            await Run(async a => { await a.LogoutAsync(session.RefreshToken); return true; });
        await Assert.ThrowsAsync<UnauthorizedException>(() => Run(a => a.RefreshAsync(new(session.RefreshToken))));
        await Assert.ThrowsAsync<UnauthorizedException>(() => Run(a => a.LoginAsync(new(email, "wrong"))));
        await Assert.ThrowsAsync<ConflictException>(() => Run(a => a.RegisterAsync(new(email, "Test1234!", "Duplicate"))));
    }

    private sealed class AnonymousUser : ICurrentUser
    {
        public Guid? UserId => null;
        public UserRole? Role => null;
        public bool IsAuthenticated => false;
        public Guid RequireUserId() => throw new UnauthorizedException("Требуется вход.");
    }
}
