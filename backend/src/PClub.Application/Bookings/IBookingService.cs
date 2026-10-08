namespace PClub.Application.Bookings
{
    /// <summary>Операции над бронями.</summary>
    public interface IBookingService
    {
        /// <summary>Создаёт бронь.</summary>
        /// <param name="request">Что и на когда бронируем.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Созданная бронь.</returns>
        /// <exception cref="Domain.Exceptions.NotFoundException">Места нет.</exception>
        /// <exception cref="Domain.Exceptions.ConflictException">Место занято или недоступно.</exception>
        Task<BookingDto> CreateAsync(
            CreateBookingRequest request, CancellationToken cancellationToken = default);

        /// <summary>Возвращает бронь.</summary>
        /// <param name="id">Идентификатор брони.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Найденная бронь.</returns>
        /// <exception cref="Domain.Exceptions.NotFoundException">Брони нет.</exception>
        Task<BookingDto> GetByIdAsync(
            Guid id, CancellationToken cancellationToken = default);

        /// <summary>Отменяет бронь.</summary>
        /// <param name="bookingId">Идентификатор брони.</param>
        /// <param name="userId">Кто отменяет.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Отменённая бронь.</returns>
        /// <exception cref="Domain.Exceptions.NotFoundException">Брони нет.</exception>
        /// <exception cref="Domain.Exceptions.ForbiddenException">Бронь чужая.</exception>
        /// <exception cref="Domain.Exceptions.ConflictException">Окно отмены закрыто.</exception>
        Task<BookingDto> CancelAsync(
            Guid bookingId, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>Занятые интервалы места на дату.</summary>
        /// <param name="seatId">Идентификатор места.</param>
        /// <param name="date">Дата.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Часы работы клуба и занятые интервалы.</returns>
        /// <exception cref="Domain.Exceptions.NotFoundException">Места нет.</exception>
        Task<SeatAvailabilityDto> GetAvailabilityAsync(
            Guid seatId, DateOnly date, CancellationToken cancellationToken = default);
    }
}
