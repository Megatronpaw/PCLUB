using PClub.Domain.Exceptions;

namespace PClub.Domain.Entities
{
    /// <summary>Пользователь сервиса.</summary>
    public sealed class User
    {
        /// <summary>Максимальная сумма разового пополнения: 100 000 рублей.</summary>
        public const long MaxTopUpCents = 10_000_000;

        /// <summary>Баланс кошелька в копейках. Никогда не отрицательный.</summary>
        public long BalanceCents { get; private set; }

        /// <summary>
        /// Начисляет средства на кошелёк.
        /// </summary>
        /// <param name="amountCents">Сумма в копейках, больше нуля.</param>
        /// <returns>Баланс после начисления.</returns>
        /// <exception cref="DomainValidationException">Сумма вне допустимых границ.</exception>
        public long Credit(long amountCents)
        {
            if (amountCents <= 0)
            {
                throw new DomainValidationException("Сумма начисления должна быть больше нуля.");
            }


            BalanceCents = checked(BalanceCents + amountCents);

            return BalanceCents;
        }

        /// <summary>
        /// Списывает средства с кошелька.
        /// </summary>
        /// <param name="amountCents">Сумма в копейках, больше нуля.</param>
        /// <returns>Баланс после списания.</returns>
        /// <exception cref="DomainValidationException">Сумма не положительна.</exception>
        /// <exception cref="InsufficientFundsException">Не хватает средств.</exception>
        public long Debit(long amountCents)
        {
            if (amountCents <= 0)
            {
                throw new DomainValidationException("Сумма списания должна быть больше нуля.");
            }
            if (amountCents > BalanceCents)
            {
                throw new InsufficientFundsException(BalanceCents, amountCents);
            }

            BalanceCents -= amountCents;

            return BalanceCents;
        }

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
