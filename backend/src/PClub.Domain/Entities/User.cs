using PClub.Domain.Exceptions;

namespace PClub.Domain.Entities
{
    /// <summary>Пользователь сервиса.</summary>
    public sealed class User
    {
        /// <summary>Максимальная длина отображаемого имени.</summary>
        public const int MaxDisplayNameLength = 100;

        private readonly List<Booking> _bookings = [];

        /// <summary>Создаёт пользователя.</summary>
        /// <param name="email">Адрес почты.</param>
        /// <param name="displayName">Отображаемое имя.</param>
        /// <exception cref="DomainValidationException">Данные не прошли проверку.</exception>
        public User(string email, string displayName)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new DomainValidationException("Почта обязательна.");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new DomainValidationException("Имя обязательно.");
            }

            if (displayName.Length > MaxDisplayNameLength)
            {
                throw new DomainValidationException(
                    $"Имя длиннее {MaxDisplayNameLength} символов.");
            }

            Id = Guid.NewGuid();

            // Почта хранится в нижнем регистре: иначе ivan@ и Ivan@ прошли бы
            // мимо уникального индекса как два разных адреса.
            Email = email.Trim().ToLowerInvariant();
            DisplayName = displayName.Trim();
            CreatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>Для EF Core.</summary>
        private User()
        {
            Email = null!;
            DisplayName = null!;
        }

        /// <summary>Идентификатор.</summary>
        public Guid Id { get; private set; }

        /// <summary>Адрес почты в нижнем регистре.</summary>
        public string Email { get; private set; }

        /// <summary>Отображаемое имя.</summary>
        public string DisplayName { get; private set; }

        /// <summary>Когда зарегистрирован.</summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>Брони этого пользователя.</summary>
        public IReadOnlyCollection<Booking> Bookings => _bookings;
    }
}
