using PClub.Application.Abstractions;
using PClub.Application.Clubs;
using PClub.Domain.Entities;
using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Application.Bookings
{
    /// <summary>Операции над бронями.</summary>
    public sealed class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookings;
        private readonly IClubRepository _clubs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        /// <summary>Создаёт сервис.</summary>
        /// <param name="bookings">Доступ к броням.</param>
        /// <param name="clubs">Доступ к клубам.</param>
        /// <param name="unitOfWork">Сохранение изменений и транзакции.</param>
        /// <param name="clock">Текущее время.</param>
        public BookingService(
            IBookingRepository bookings,
            IClubRepository clubs,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _bookings = bookings;
            _clubs = clubs;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        /// <inheritdoc />
        public Task<BookingDto> CreateAsync(
            CreateBookingRequest request, CancellationToken cancellationToken = default) =>
            _unitOfWork.ExecuteInTransactionAsync(
                token => CreateCoreAsync(request, token), cancellationToken);

        /// <inheritdoc />
        public async Task<BookingDto> GetByIdAsync(
            Guid id, CancellationToken cancellationToken = default)
        {
            var booking = await _bookings.GetTrackedAsync(id, cancellationToken)
                ?? throw new NotFoundException("Booking", id);

            return BookingDto.From(booking);
        }

        /// <inheritdoc />
        public async Task<BookingDto> CancelAsync(
            Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookings.GetTrackedAsync(bookingId, cancellationToken)
                ?? throw new NotFoundException("Booking", bookingId);

            if (booking.UserId != userId)
            {
                throw new ForbiddenException("Это не ваша бронь.");
            }

            booking.Cancel(_clock.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return BookingDto.From(booking);
        }

        /// <inheritdoc />
        public async Task<SeatAvailabilityDto> GetAvailabilityAsync(
            Guid seatId, DateOnly date, CancellationToken cancellationToken = default)
        {
            var seat = await _clubs.GetSeatWithContextAsync(seatId, cancellationToken)
                ?? throw new NotFoundException("Seat", seatId);

            var club = seat.Zone.Club;

            var from = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var to = from.AddDays(1);

            var busy = await _bookings.GetBusyIntervalsAsync(
                seatId, from, to, cancellationToken);

            return new SeatAvailabilityDto(
                seat.Id,
                seat.Label,
                date,
                club.OpeningTime,
                club.ClosingTime,
                busy);
        }

        /// <summary>
        /// Создаёт бронь. Вызывается внутри транзакции и может быть повторён.
        /// </summary>
        /// <param name="request">Что и на когда бронируем.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Созданная бронь.</returns>
        private async Task<BookingDto> CreateCoreAsync(
            CreateBookingRequest request, CancellationToken cancellationToken)
        {
            var seat = await _clubs.GetSeatWithContextAsync(request.SeatId, cancellationToken)
                ?? throw new NotFoundException("Seat", request.SeatId);

            var zone = seat.Zone;
            var club = zone.Club;

            if (seat.Status != SeatStatus.Active)
            {
                throw new ConflictException(
                    $"Место {seat.Label} сейчас недоступно для брони.");
            }

            if (club.Status != ClubStatus.Published)
            {
                throw new ConflictException("Клуб недоступен для брони.");
            }

            if (!club.IsWithinOpeningHours(request.StartTime, request.EndTime))
            {
                throw new DomainValidationException(
                    $"Клуб работает с {club.OpeningTime} до {club.ClosingTime}.");
            }

            if (await _bookings.HasOverlapAsync(
                request.SeatId, request.StartTime, request.EndTime, cancellationToken))
            {
                throw new ConflictException(
                    $"Место {seat.Label} уже занято в выбранное время.");
            }

            var booking = new Booking(
                userId: request.UserId,
                clubId: club.Id,
                zoneId: zone.Id,
                seatId: seat.Id,
                startTime: request.StartTime,
                endTime: request.EndTime,
                pricePerHourCents: zone.PricePerHourCents,
                now: _clock.UtcNow);

            _bookings.Add(booking);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return BookingDto.From(booking);
        }
    }
}
