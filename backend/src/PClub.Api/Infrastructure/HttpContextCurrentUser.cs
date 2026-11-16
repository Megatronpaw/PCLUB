using System.Security.Claims;
using PClub.Application.Abstractions;
using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Api.Infrastructure;

/// <summary>Текущий пользователь из заявок токена.</summary>
public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>Создаёт обёртку.</summary>
    public HttpContextCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var raw = _accessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public UserRole? Role
    {
        get
        {
            var raw = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);

            return Enum.TryParse<UserRole>(raw, out var role) ? role : null;
        }
    }

    /// <inheritdoc />
    public bool IsAuthenticated =>
        _accessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public Guid RequireUserId() =>
        IsAuthenticated && UserId is { } id && id != Guid.Empty
            ? id : throw new UnauthorizedException("Требуется вход.");
}
