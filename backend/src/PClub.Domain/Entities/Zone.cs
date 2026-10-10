using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Domain.Entities
{
    /// <summary>
    /// Зона внутри клуба: группа мест с общей ценой.
    /// </summary>
    public sealed class Zone
    {
        private readonly List<Seat> _seats = [];

        /// <summary>Создаёт зону.</summary>
        /// <param name="name">Название, например Standard или VIP.</param>
        /// <param name="pricePerHourCents">Ставка за час в копейках, больше нуля.</param>
        /// <param name="specs">Описание железа зоны.</param>
        /// <exception cref="DomainValidationException">Название пустое или ставка не положительная.</exception>
        public Zone(string name, long pricePerHourCents, string specs = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainValidationException("У зоны должно быть название.");
            }

            if (pricePerHourCents <= 0)
            {
                throw new DomainValidationException("Цена за час должна быть больше нуля.");
            }

            Id = Guid.NewGuid();
            Name = name.Trim();
            PricePerHourCents = pricePerHourCents;

            Specs = specs.Trim();
        }

        /// <summary>Для EF Core.</summary>
        private Zone() => Name = null!;

        /// <summary>Идентификатор.</summary>
        public Guid Id { get; private set; }

        /// <summary>Клуб, к которому относится зона.</summary>
        public Guid ClubId { get; private set; }

        /// <summary>Название зоны.</summary>
        public string Name { get; private set; }

        /// <summary>Ставка за час в копейках.</summary>
        public long PricePerHourCents { get; private set; }

        /// <summary>Описание железа.</summary>
        public string Specs { get; private set; } = string.Empty;

        /// <summary>Места этой зоны. Только для чтения.</summary>
        public IReadOnlyCollection<Seat> Seats => _seats;

        /// <summary>Сколько мест можно бронировать прямо сейчас.</summary>
        public int ActiveSeatsCount => _seats.Count(s => s.Status == SeatStatus.Active);

        /// <summary>Добавляет место в зону.</summary>
        /// <param name="seat">Место.</param>
        /// <exception cref="ConflictException">Место с такой меткой в зоне уже есть.</exception>
        public void AddSeat(Seat seat)
        {
            if (_seats.Any(s => s.Label == seat.Label))
            {
                throw new ConflictException($"Место «{seat.Label}» в этой зоне уже есть.");
            }

            _seats.Add(seat);
        }

        /// <summary>Меняет ставку за час.</summary>
        /// <param name="pricePerHourCents">Новая ставка в копейках, больше нуля.</param>
        /// <exception cref="DomainValidationException">Ставка не положительная.</exception>
        public void ChangePrice(long pricePerHourCents)
        {
            if (pricePerHourCents <= 0)
            {
                throw new DomainValidationException("Цена за час должна быть больше нуля.");
            }

            PricePerHourCents = pricePerHourCents;
        }

        /// <summary>Клуб, которому принадлежит зона.</summary>
        public Club Club { get; private set; } = null!;
    }
}
