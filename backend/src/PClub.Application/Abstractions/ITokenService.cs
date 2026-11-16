using PClub.Domain.Entities;

namespace PClub.Application.Abstractions;

/// <summary>Выпуск токенов.</summary>
public interface ITokenService
{
    /// <summary>Выпускает токен доступа.</summary>
    /// <returns>Токен и момент истечения.</returns>
    (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(User user);

    /// <summary>Выпускает токен обновления.</summary>
    /// <returns>Значение для клиента и хеш для базы.</returns>
    (string Value, string Hash) CreateRefreshToken();

    /// <summary>Считает хеш значения токена обновления.</summary>
    string HashRefreshToken(string value);
}
