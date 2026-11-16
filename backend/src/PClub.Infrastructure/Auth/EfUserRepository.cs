using Microsoft.EntityFrameworkCore;
using PClub.Application.Auth;
using PClub.Domain.Entities;
using PClub.Infrastructure.Persistence;

namespace PClub.Infrastructure.Auth;

/// <summary>Доступ к пользователям через EF Core.</summary>
public sealed class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    /// <summary>Создаёт репозиторий.</summary>
    public EfUserRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public Task<User?> GetByEmailAsync(
        string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    /// <inheritdoc />
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<bool> EmailExistsAsync(
        string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return _db.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
    }

    /// <inheritdoc />
    public void Add(User user) => _db.Users.Add(user);
}

/// <summary>Доступ к токенам обновления через EF Core.</summary>
public sealed class EfRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    /// <summary>Создаёт репозиторий.</summary>
    public EfRefreshTokenRepository(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public Task<RefreshToken?> GetByHashAsync(
        string tokenHash, CancellationToken cancellationToken = default) =>
        _db.RefreshTokens.FirstOrDefaultAsync(
            t => t.TokenHash == tokenHash, cancellationToken);

    /// <inheritdoc />
    public async Task RevokeFamilyAsync(
        Guid familyId, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        var tokens = await _db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke(now);
        }
    }

    /// <inheritdoc />
    public void Add(RefreshToken token) => _db.RefreshTokens.Add(token);
}
