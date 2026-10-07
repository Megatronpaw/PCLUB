using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Domain.Entities
{
    /// <summary>
    /// Компьютерный клуб.
    /// </summary>
    public sealed class Club
    {
        private readonly List<Zone> _zones = [];

        /// <summary>Создаёт клуб. Новый клуб всегда черновик.</summary>
        /// <param name="name">Название.</param>
        /// <param name="city">Город.</param>
        /// <param name="address">Адрес.</param>
        /// <param name="openingTime">Время открытия.</param>
        /// <param name="closingTime">Время закрытия. Равно открытию — значит круглосуточно.</param>
        /// <param name="description">Описание клуба для карточки в каталоге.</param>
        /// <exception cref="DomainValidationException">Название или город пустые.</exception>
        public Club(
            string name, string city, string address,
            TimeOnly openingTime, TimeOnly closingTime, string description = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainValidationException("У клуба должно быть название.");
            }

            if (string.IsNullOrWhiteSpace(city))
            {
                throw new DomainValidationException("У клуба должен быть город.");
            }

            Id = Guid.NewGuid();
            Name = name.Trim();
            City = city.Trim();
            Address = address.Trim();
            OpeningTime = openingTime;
            ClosingTime = closingTime;
            Status = ClubStatus.Draft;
            CreatedAt = DateTimeOffset.UtcNow;
            Description = description.Trim();
            
        }

        /// <summary>Идентификатор.</summary>
        public Guid Id { get; private set; }

        /// <summary>Название.</summary>
        public string Name { get; private set; } = null!;

        /// <summary>Город.</summary>
        public string City { get; private set; } = null!;

        /// <summary>Адрес.</summary>
        public string Address { get; private set; } = string.Empty;

        /// <summary>Время открытия.</summary>
        public TimeOnly OpeningTime { get; private set; }

        /// <summary>Время закрытия.</summary>
        public TimeOnly ClosingTime { get; private set; }

        /// <summary>Состояние в каталоге.</summary>
        public ClubStatus Status { get; private set; }

        /// <summary>Когда создан.</summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>Зоны клуба. Только для чтения.</summary>
        public IReadOnlyCollection<Zone> Zones => _zones;

        /// <summary>Работает ли клуб круглосуточно.</summary>
        public bool IsOpenAllDay => OpeningTime == ClosingTime;

        /// <summary>Добавляет зону в клуб.</summary>
        /// <param name="zone">Зона.</param>
        public void AddZone(Zone zone) => _zones.Add(zone);

        /// <summary>
        ///Описание для клуба 
        /// </summary>
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Бронь только в рабочие часы клуба.
        /// </summary>
        /// <param name="start">Начало интервала.</param>
        /// <param name="end">Окончание интервала.</param>
        /// <returns><c>true</c>, если интервал укладывается в часы работы.</returns>
        public bool IsWithinOpeningHours(DateTimeOffset start, DateTimeOffset end)
        {
            if (IsOpenAllDay)
            {
                return true;
            }

            var from = TimeOnly.FromDateTime(start.UtcDateTime);
            var to = TimeOnly.FromDateTime(end.UtcDateTime);

            var crossesMidnight = ClosingTime <= OpeningTime;

            return crossesMidnight
                ? IsInsideOvernight(from) && IsInsideOvernight(to)
                : from >= OpeningTime && to <= ClosingTime;
        }

        private bool IsInsideOvernight(TimeOnly time) =>
            time >= OpeningTime || time <= ClosingTime;

        /// <summary>Меняет состояние клуба в каталоге.</summary>
        /// <param name="status">Новое состояние.</param>
        /// <param name="hasBookableSeats">Есть ли в клубе хотя бы одно активное место.</param>
        /// <exception cref="ConflictException">Публикация клуба без активных мест.</exception>
        /// <remarks>
        /// Факт «есть активные места» приходит параметром, а не считается здесь:
        /// клуб знает своё правило, но не обязан знать, как посчитать места.
        /// </remarks>
        public void ChangeStatus(ClubStatus status, bool hasBookableSeats)
        {
            if (status == ClubStatus.Published && !hasBookableSeats)
            {
                throw new ConflictException(
                    "Нельзя опубликовать клуб, в котором нет ни одного активного места.");
            }

            Status = status;
        }
    }
}
