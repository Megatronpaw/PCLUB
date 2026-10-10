using Microsoft.EntityFrameworkCore;
using PClub.Application.Abstractions;
using PClub.Application.Clubs;
using PClub.Application.Common;
using PClub.Domain.Entities;
using PClub.Domain.Enums;
using PClub.Infrastructure.Persistence;

namespace PClub.Infrastructure.Clubs
{
    /// <summary>
    /// Клубы из PostgreSQL.
    /// </summary>
    public sealed class EfClubRepository : IClubRepository
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;

        /// <summary>
        /// Создаёт репозиторий.
        /// </summary>
        /// <param name="db">Контекст базы.</param>
        /// <param name="clock">Текущее время: нужно фильтру «открыт сейчас».</param>
        public EfClubRepository(AppDbContext db, IClock clock)
        {
            _db = db;
            _clock = clock;
        }
        ///<inheritdoc/>
        public async Task<IReadOnlyList<Club>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            await _db.Clubs
            .AsNoTracking()
            .Include(c => c.Zones)
            .ThenInclude(z => z.Seats)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        ///<inheritdoc/>
        public async Task<Club?> GetByIdAsync(
            Guid id, CancellationToken cancellationToken = default) =>
            await _db.Clubs
                .AsNoTracking()
            .Include(c => c.Zones)
            .ThenInclude(z => z.Seats)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        ///<inheritdoc/>
        public async Task<Zone?> GetZoneByIdAsync(
            Guid zoneId, CancellationToken cancellation = default) =>
            await _db.Zones
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.Id == zoneId, cancellation);

        ///<inheritdoc/>
        public async Task<Club?> GetClubByZoneIdAsync(
            Guid zoneId, CancellationToken cancellationToken = default) =>
            await _db.Clubs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Zones.Any(z => z.Id == zoneId), cancellationToken);

        ///<inheritdoc/>
        public async Task<Club?> GetTrackedAsync(
            Guid id, CancellationToken cancellationToken = default) =>
            await _db.Clubs
                .Include(c => c.Zones)
                .ThenInclude(z => z.Seats)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        ///<inheritdoc/>
        public async Task<Zone?> GetTrackedZoneAsync(
            Guid zoneId, CancellationToken cancellationToken = default) =>
            await _db.Zones
                .Include(z => z.Seats)
                .FirstOrDefaultAsync(z => z.Id == zoneId, cancellationToken);

        ///<inheritdoc/>
        public async Task<Seat?> GetSeatWithContextAsync(
            Guid seatId, CancellationToken cancellationToken = default) =>
            await _db.Seats
                .AsNoTracking()
                .Include(s => s.Zone)
                    .ThenInclude(z => z.Club)
                .FirstOrDefaultAsync(s => s.Id == seatId, cancellationToken);

        /// <inheritdoc />
        public async Task<PagedResult<ClubListItemDto>> SearchAsync(
            ClubCatalogQuery query, CancellationToken cancellationToken = default)
        {
            var clubs = _db.Clubs
                .AsNoTracking()
                .Where(c => c.Status == ClubStatus.Published);

            if (!string.IsNullOrWhiteSpace(query.City))
            {
                clubs = clubs.Where(c => c.City == query.City);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var pattern = $"%{query.Search.Trim()}%";

                clubs = clubs.Where(c =>
                EF.Functions.ILike(c.Name, pattern) ||
                EF.Functions.ILike(c.Address, pattern));
            }

            if (query.MinPricePerHourCents is { } min)
            {
                clubs = clubs.Where(c => c.Zones.Any(z => z.PricePerHourCents >= min));
            }

            if (query.MaxPricePerHourCents is { } max)
            {
                clubs = clubs.Where(c => c.Zones.Any(z => z.PricePerHourCents <= max));
            }

            if (query.OpenNow == true)
            {
                var now = TimeOnly.FromDateTime(_clock.UtcNow.UtcDateTime);

                clubs = clubs.Where(c => c.OpeningTime <= now && now < c.ClosingTime);
            }

            if (query.HasInterval)
            {
                var from = query.From!.Value;
                var to = query.To!.Value;
                var needed = query.NormalizedSeatsNeeded;

                var busySeatIds = _db.Bookings
                    .Where(b => b.Status != BookingStatus.Cancelled)
                    .Where(b => from < b.EndTime && b.StartTime < to)
                    .Select(b => b.SeatId);

                clubs = clubs.Where(c => c.Zones
                    .SelectMany(z => z.Seats)
                    .Count(s => s.Status == SeatStatus.Active
                             && !busySeatIds.Contains(s.Id)) >= needed);
            }

            var totalCount = await clubs.CountAsync(cancellationToken);

            clubs = query.Sort switch
            {
                "price_asc" => clubs.OrderBy(c => c.Zones.Min(z => z.PricePerHourCents)),
                "price_desc" => clubs.OrderByDescending(c => c.Zones.Min(z => z.PricePerHourCents)),
                "seats_desc" => clubs.OrderByDescending(c => c.Zones.Sum(z => z.Seats.Count)),

                _ => clubs.OrderBy(c => c.Name),
            };

            var items = await clubs
                .Skip((query.NormalizedPage - 1) * query.NormalizedPageSize)
                .Take(query.NormalizedPageSize)
                .Select(c => new ClubListItemDto(
                    c.Id,
                    c.Name,
                    c.City,
                    c.Address,
                    c.Zones.Any() ? c.Zones.Min(z => z.PricePerHourCents) : 0,
                    c.Zones.Any() ? c.Zones.Max(z => z.PricePerHourCents) : 0,
                    c.Zones.Count,
                    c.Zones.Sum(z => z.Seats.Count(s => s.Status == SeatStatus.Active))))
                .ToListAsync(cancellationToken);
            return new PagedResult<ClubListItemDto>(
                items, query.NormalizedPage, query.NormalizedPageSize, totalCount);
        }
    }
}
