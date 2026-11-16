using PClub.Domain.Enums;

namespace PClub.Application.Abstractions;

/// <summary>
/// Кто выполняет текущий запрос.
/// </summary>
/// <remarks>
/// Абстракция нужна, чтобы слой Application не знал про HttpContext.
/// В тесте подставляется простая заглушка с нужным идентификатором.
/// </remarks>
public interface ICurrentUser
{
    /// <summary>Идентификатор, или null для анонимного запроса.</summary>
    Guid? UserId { get; }

    /// <summary>Роль, или null для анонимного запроса.</summary>
    UserRole? Role { get; }

    /// <summary>Подтверждена ли личность.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Возвращает идентификатор или бросает исключение.</summary>
    /// <exception cref="Domain.Exceptions.UnauthorizedException">Запрос анонимный.</exception>
    Guid RequireUserId();
}
