using PClub.Domain.Enums;

namespace PClub.Application.Auth;

/// <summary>Текущий пользователь в ответе.</summary>
/// <param name="UserId">Идентификатор.</param>
/// <param name="Email">Почта.</param>
/// <param name="DisplayName">Имя.</param>
/// <param name="Role">Роль.</param>
/// <param name="BalanceCents">Баланс кошелька.</param>
public sealed record CurrentUserDto(
    Guid UserId, string Email, string DisplayName, UserRole Role, long BalanceCents);

/// <summary>Регистрация, вход и обновление токенов.</summary>
public interface IAuthService
{
    /// <summary>Регистрирует пользователя и выдаёт токены.</summary>
    /// <exception cref="Domain.Exceptions.ConflictException">Почта занята.</exception>
    Task<AuthResultDto> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Вход по почте и паролю.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Неверные данные.</exception>
    Task<AuthResultDto> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Обменивает токен обновления на новую пару.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Токен недействителен.</exception>
    Task<AuthResultDto> RefreshAsync(
        RefreshRequest request, CancellationToken cancellationToken = default);

    /// <summary>Завершает все сеансы цепочки. Идемпотентно.</summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Данные текущего пользователя.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Вход не выполнен.</exception>
    Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
