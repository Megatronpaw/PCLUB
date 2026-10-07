using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Domain.Entities
{
    /// <summary>
    /// Бронь места на интервал времени.
    /// </summary>
    public sealed class Booking
    {
        /// <summary>Сетка продаж: только целые полчаса.</summary>
        public static readonly TimeSpan SlotGranularity = TimeSpan.FromMinutes(30);

        /// <summary>За сколько до начала ещё можно отменить.</summary>
        public static readonly TimeSpan CancellationCutoff = TimeSpan.FromHours(2);

        /// <summary>Максимальная длительность одной брони.</summary>
        public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(12);

        /// <summary>Создаёт бронь.</summary>
        /// <param name="seatId">Место.</param>
        /// <param name="clubId">Клуб.</param>
        /// <param name="startTime">Начало, UTC. На границе получаса.</param>
        /// <param name="endTime">Конец, UTC. В интервал не включается.</param>
        /// <param name="totalPriceCents">Итоговая стоимость в копейках, больше нуля.</param>
        /// <exception cref="DomainValidationException">Интервал или стоимость не по правилам.</exception>
        public Booking(
            Guid seatId, Guid clubId,
            DateTimeOffset startTime, DateTimeOffset endTime, long totalPriceCents)
        {
            ValidateRange(startTime, endTime);

            if (totalPriceCents <= 0)
            {
                throw new DomainValidationException("Стоимость брони должна быть больше нуля.");
            }

            Id = Guid.NewGuid();
            SeatId = seatId;
            ClubId = clubId;
            StartTime = startTime;
            EndTime = endTime;
            TotalPriceCents = totalPriceCents;
            Status = BookingStatus.Confirmed;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>Для EF Core.</summary>
        private Booking()
        {
        }

        /// <summary>Идентификатор.</summary>
        public Guid Id { get; private set; }

        /// <summary>Место.</summary>
        public Guid SeatId { get; private set; }

        /// <summary>Клуб.</summary>
        public Guid ClubId { get; private set; }

        /// <summary>Начало брони.</summary>
        public DateTimeOffset StartTime { get; private set; }

        /// <summary>Конец брони, в интервал не включается.</summary>
        public DateTimeOffset EndTime { get; private set; }

        /// <summary>Состояние.</summary>
        public BookingStatus Status { get; private set; }

        /// <summary>Стоимость в копейках.</summary>
        public long TotalPriceCents { get; private set; }

        /// <summary>Когда создана.</summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>Когда отменена; null, если не отменена.</summary>
        // Тип с вопросительным знаком: без него «не отменена» выглядело бы
        // как 0001-01-01, и отличить её от настоящей даты было бы нельзя.
        public DateTimeOffset? CancelledAt { get; private set; }

        /// <summary>Длительность брони.</summary>
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>Считает стоимость по ставке зоны.</summary>
        /// <param name="pricePerHourCents">Ставка за час в копейках.</param>
        /// <param name="start">Начало интервала.</param>
        /// <param name="end">Конец интервала.</param>
        public static long CalculatePrice(
            long pricePerHourCents, DateTimeOffset start, DateTimeOffset end) =>
            Money.ForDuration(pricePerHourCents, end - start);

        /// <summary>Пересекается ли эта бронь с другой.</summary>
        /// <param name="other">Другая бронь.</param>
        /// <remarks>
        /// Неравенства строгие: интервал полуоткрытый, поэтому 14:00–15:00
        /// и 15:00–16:00 конфликтом не считаются.
        /// </remarks>
        public bool Overlaps(Booking other) =>
            SeatId == other.SeatId
            && StartTime < other.EndTime
            && other.StartTime < EndTime;

        /// <summary>Можно ли отменить бронь в указанный момент.</summary>
        /// <param name="now">Текущий момент.</param>
        public bool CanBeCancelled(DateTimeOffset now) =>
            Status == BookingStatus.Confirmed
            && now <= StartTime - CancellationCutoff;

        /// <summary>Отменяет бронь.</summary>
        /// <param name="now">Текущий момент.</param>
        /// <exception cref="ConflictException">Бронь уже отменена или окно отмены закрыто.</exception>
        public void Cancel(DateTimeOffset now)
        {
            if (Status == BookingStatus.Cancelled)
            {
                throw new ConflictException("Бронь уже отменена.");
            }

            if (!CanBeCancelled(now))
            {
                throw new ConflictException(
                    $"Отмена закрывается за {CancellationCutoff.TotalHours:0} ч до начала.");
            }

            Status = BookingStatus.Cancelled;
            CancelledAt = now;
        }

        /// <summary>Помечает бронь завершённой.</summary>
        public void MarkCompleted() => Status = BookingStatus.Completed;

        /// <summary>Проверяет интервал брони.</summary>
        /// <param name="start">Начало.</param>
        /// <param name="end">Конец.</param>
        /// <exception cref="DomainValidationException">Интервал не соответствует правилам.</exception>
        public static void ValidateRange(DateTimeOffset start, DateTimeOffset end)
        {
            if (end <= start)
            {
                throw new DomainValidationException("Конец брони должен быть позже начала.");
            }

            var duration = end - start;

            if (duration > MaxDuration)
            {
                throw new DomainValidationException(
                    $"Одна бронь не может быть длиннее {MaxDuration.TotalHours:0} ч.");
            }

            // По Ticks, а не по TotalMinutes: TotalMinutes — double,
            // и остаток там может оказаться 1E-14 вместо нуля.
            if (duration.Ticks % SlotGranularity.Ticks != 0)
            {
                throw new DomainValidationException(
                    $"Длительность должна быть кратна {SlotGranularity.TotalMinutes:0} минутам.");
            }

            if (start.Minute % SlotGranularity.TotalMinutes != 0
                || start.Second != 0
                || start.Millisecond != 0)
            {
                throw new DomainValidationException(
                    $"Бронь должна начинаться на границе {SlotGranularity.TotalMinutes:0} минут.");
            }
        }
    }
}
