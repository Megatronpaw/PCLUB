using PClub.Domain.Enums;

namespace PClub.Application.Auth;

/// <summary>Запрос регистрации.</summary>
/// <param name="Email">Адрес почты.</param>
/// <param name="Password">Пароль.</param>
/// <param name="DisplayName">Как показывать имя.</param>
public sealed record RegisterRequest(string Email, string Password, string DisplayName);

/// <summary>Запрос входа.</summary>
/// <param name="Email">Адрес почты.</param>
/// <param name="Password">Пароль.</param>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Запрос обновления токенов.</summary>
/// <param name="RefreshToken">Значение токена обновления.</param>
public sealed record RefreshRequest(string RefreshToken);

/// <summary>Пара токенов и кто вошёл.</summary>
/// <param name="AccessToken">Токен доступа.</param>
/// <param name="RefreshToken">Токен обновления.</param>
/// <param name="AccessTokenExpiresAt">Когда истекает токен доступа.</param>
/// <param name="UserId">Кто.</param>
/// <param name="Email">Почта.</param>
/// <param name="DisplayName">Имя.</param>
/// <param name="Role">Роль.</param>
public sealed record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    Guid UserId,
    string Email,
    string DisplayName,
    UserRole Role);
