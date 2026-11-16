using PClub.Domain.Entities;

namespace PClub.Application.Auth;

/// <summary>Доступ к пользователям.</summary>
public interface IUserRepository
{
    /// <summary>Пользователь по почте, отслеживаемый.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Пользователь по идентификатору, отслеживаемый.</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Занята ли почта.</summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Добавляет пользователя.</summary>
    void Add(User user);
}

/// <summary>Доступ к токенам обновления.</summary>
public interface IRefreshTokenRepository
{
    /// <summary>Токен по хешу значения, отслеживаемый.</summary>
    Task<RefreshToken?> GetByHashAsync(
        string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Отзывает все токены цепочки.</summary>
    Task RevokeFamilyAsync(
        Guid familyId, DateTimeOffset now, CancellationToken cancellationToken = default);

    /// <summary>Добавляет токен.</summary>
    void Add(RefreshToken token);
}
