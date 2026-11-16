using PClub.Domain.Exceptions;

namespace PClub.Domain.Entities;

/// <summary>
/// Долгоживущий токен обновления. Хранится в базе, поэтому отзываем.
/// </summary>
public class RefreshToken
{
    /// <summary>Сколько живёт токен обновления.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    /// <summary>Создаёт токен.</summary>
    /// <param name="userId">Владелец.</param>
    /// <param name="tokenHash">Хеш значения токена.</param>
    /// <param name="familyId">Цепочка: одна на все обновления одного входа.</param>
    /// <param name="now">Текущий момент.</param>
    public RefreshToken(Guid userId, string tokenHash, Guid familyId, DateTimeOffset now)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("Владелец токена обязателен.");
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new DomainValidationException("Хеш токена обязателен.");
        }

        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        FamilyId = familyId;
        CreatedAt = now;
        ExpiresAt = now + Lifetime;
    }

    /// <summary>Для EF Core.</summary>
    private RefreshToken() => TokenHash = null!;

    /// <summary>Идентификатор.</summary>
    public Guid Id { get; private set; }

    /// <summary>Владелец.</summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Хеш значения токена.
    /// </summary>
    /// <remarks>
    /// Именно ХЕШ, а не само значение. Refresh-токен — это пароль на
    /// тридцать дней: при утечке базы хранившиеся открыто токены позволили
    /// бы войти под любым пользователем. Хеш — нет.
    /// </remarks>
    public string TokenHash { get; private set; }

    /// <summary>Цепочка обновлений одного входа.</summary>
    public Guid FamilyId { get; private set; }

    /// <summary>Когда создан.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Когда истекает.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>Когда использован для обновления.</summary>
    public DateTimeOffset? UsedAt { get; private set; }

    /// <summary>Когда отозван.</summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>Годен ли токен к использованию.</summary>
    public bool IsActive(DateTimeOffset now) =>
        UsedAt is null && RevokedAt is null && now < ExpiresAt;

    /// <summary>Помечает токен использованным.</summary>
    /// <exception cref="ConflictException">Токен уже использован или отозван.</exception>
    public void MarkUsed(DateTimeOffset now)
    {
        if (!IsActive(now))
        {
            throw new ConflictException("Токен обновления недействителен.");
        }

        UsedAt = now;
    }

    /// <summary>Отзывает токен.</summary>
    /// <remarks>
    /// Повторный отзыв не ошибка: операция идемпотентна. Это важно при
    /// отзыве всей цепочки — часть токенов может быть уже отозвана.
    /// </remarks>
    public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
}
