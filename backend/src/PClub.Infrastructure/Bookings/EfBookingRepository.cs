using Microsoft.EntityFrameworkCore;
using PClub.Application.Bookings;
using PClub.Domain.Entities;
using PClub.Domain.Enums;
using PClub.Infrastructure.Persistence;

namespace PClub.Infrastructure.Bookings
{
    /// <summary>Доступ к броням через EF Core.</summary>
    public sealed class EfBookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        /// <summary>Создаёт репозиторий.</summary>
        /// <param name="db">Контекст базы.</param>
        public EfBookingRepository(AppDbContext db) => _db = db;

        /// <inheritdoc />
        public async Task<Booking?> GetTrackedAsync(
            Guid id, CancellationToken cancellationToken = default) =>
            await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        /// <inheritdoc />
        public Task<bool> HasOverlapAsync(
            Guid seatId,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            CancellationToken cancellationToken = default) =>
            _db.Bookings
                .AsNoTracking()
                .Where(b => b.SeatId == seatId)
                // Отменённая бронь место не занимает.
                .Where(b => b.Status != BookingStatus.Cancelled)
                // Та же формула, что в Booking.Overlaps, но в SQL.
                .AnyAsync(b => startTime < b.EndTime && b.StartTime < endTime,
                    cancellationToken);

        /// <inheritdoc />
        public async Task<IReadOnlyList<BusyIntervalDto>> GetBusyIntervalsAsync(
            Guid seatId,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default) =>
            await _db.Bookings
                .AsNoTracking()
                .Where(b => b.SeatId == seatId)
                .Where(b => b.Status != BookingStatus.Cancelled)
                // Пересечение, а не «начало внутри дня»: бронь с 23:00 до 01:00
                // относится к двум датам и должна быть видна на обеих.
                .Where(b => from < b.EndTime && b.StartTime < to)
                .OrderBy(b => b.StartTime)
                // Проекция: две колонки вместо всей брони.
                .Select(b => new BusyIntervalDto(b.StartTime, b.EndTime))
                .ToListAsync(cancellationToken);

        /// <inheritdoc />
        public void Add(Booking booking) => _db.Bookings.Add(booking);
    }
}
