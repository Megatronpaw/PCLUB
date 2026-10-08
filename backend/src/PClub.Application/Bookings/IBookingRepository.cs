using PClub.Domain.Entities;

namespace PClub.Application.Bookings
{
    /// <summary>Доступ к броням.</summary>
    public interface IBookingRepository
    {
        /// <summary>Бронь по идентификатору, отслеживаемая.</summary>
        /// <param name="id">Идентификатор брони.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Бронь или <c>null</c>, если такой нет.</returns>
        Task<Booking?> GetTrackedAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Есть ли пересекающаяся бронь на это место.</summary>
        /// <param name="seatId">Идентификатор места.</param>
        /// <param name="startTime">Начало интервала.</param>
        /// <param name="endTime">Окончание интервала.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns><c>true</c>, если место уже занято.</returns>
        Task<bool> HasOverlapAsync(
            Guid seatId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default);

        /// <summary>Занятые интервалы места, пересекающиеся с <c>[from, to)</c>.</summary>
        /// <param name="seatId">Идентификатор места.</param>
        /// <param name="from">Начало интервала поиска.</param>
        /// <param name="to">Окончание интервала поиска, не включается.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Интервалы по возрастанию начала.</returns>
        Task<IReadOnlyList<BusyIntervalDto>> GetBusyIntervalsAsync(
            Guid seatId,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default);

        /// <summary>Добавляет бронь в контекст.</summary>
        /// <param name="booking">Новая бронь.</param>
        void Add(Booking booking);
    }
}
