using PClub.Application.Wallets;
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
        private readonly ICurrentUser _currentUser;
        private readonly IClock _clock;
        private readonly IWalletRepository _wallets;

        /// <summary>Создаёт сервис.</summary>
        /// <param name="bookings">Доступ к броням.</param>
        /// <param name="clubs">Доступ к клубам.</param>
        /// <param name="currentUser">Текущий пользователь.</param>
        /// <param name="unitOfWork">Сохранение изменений и транзакции.</param>
        /// <param name="wallets">Кошельки и журнал операций.</param>
        /// <param name="clock">Текущее время.</param>
        public BookingService(
            IBookingRepository bookings,
            IClubRepository clubs,
            IUnitOfWork unitOfWork, ICurrentUser currentUser,
            IClock clock, IWalletRepository wallets)
        {
            _bookings = bookings;
            _clubs = clubs;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _clock = clock;
            _wallets = wallets;
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

            if (booking.UserId != _currentUser.RequireUserId()) throw new ForbiddenException("Это не ваша бронь.");
            return BookingDto.From(booking);
        }

        /// <inheritdoc />
        public Task<BookingDto> CancelAsync(
            Guid bookingId, CancellationToken cancellationToken = default) =>
            _unitOfWork.ExecuteInTransactionAsync(
                token => CancelCoreAsync(bookingId, token), cancellationToken);

        /// <summary>Отмена внутри транзакции.</summary>
        private async Task<BookingDto> CancelCoreAsync(
            Guid bookingId, CancellationToken cancellationToken)
        {
            var booking = await _bookings.GetTrackedAsync(bookingId, cancellationToken)
                ?? throw new NotFoundException("Booking", bookingId);

            if (booking.UserId != _currentUser.RequireUserId())
            {
                throw new ForbiddenException("Это не ваша бронь.");
            }
            var now = _clock.UtcNow;

            booking.Cancel(now);
            var refundCents = await _wallets.HasBookingChargeAsync(booking.Id, cancellationToken)
                ? booking.CalculateRefundCents(now) : 0;

            if (refundCents > 0)
            {
                var user = await _wallets.GetTrackedUserAsync(booking.UserId, cancellationToken)
                    ?? throw new NotFoundException("User", booking.UserId);

                var balanceAfter = user.Credit(refundCents);

                var isFull = refundCents == booking.TotalPriceCents;

                _wallets.AddTransaction(new WalletTransaction(
                    userId: user.Id,
                    type: WalletTransactionType.BookingRefund,
                    amountCents: refundCents,
                    balanceAfterCents: balanceAfter,
                    description: isFull
                        ? $"Полный возврат за отмену брони на {booking.StartTime:dd.MM HH:mm}"
                        : $"Возврат 50% за позднюю отмену брони на {booking.StartTime:dd.MM HH:mm}",
                    now: now,
                    bookingId: booking.Id));

                booking.MarkRefunded(refundCents);
            }

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
                userId: _currentUser.RequireUserId(),
                clubId: club.Id,
                zoneId: zone.Id,
                seatId: seat.Id,
                startTime: request.StartTime,
                endTime: request.EndTime,
                pricePerHourCents: zone.PricePerHourCents,
                now: _clock.UtcNow);

            var user = await _wallets.GetTrackedUserAsync(_currentUser.RequireUserId(), cancellationToken)
                ?? throw new NotFoundException("User", _currentUser.RequireUserId());
            var balanceAfter = user.Debit(booking.TotalPriceCents);

            _bookings.Add(booking);

            _wallets.AddTransaction(new WalletTransaction(
                userId: user.Id,
                type: WalletTransactionType.BookingCharge,
                amountCents: -booking.TotalPriceCents,
                balanceAfterCents: balanceAfter,
                description: $"Бронь места {seat.Label} на {booking.StartTime:dd.MM HH:mm}",
                now: _clock.UtcNow,
                bookingId: booking.Id));
            await _unitOfWork.SaveChangesAsync(cancellationToken);



            if (booking.UserId != _currentUser.RequireUserId()) throw new ForbiddenException("Это не ваша бронь.");
            return BookingDto.From(booking);
        }
    }
}
