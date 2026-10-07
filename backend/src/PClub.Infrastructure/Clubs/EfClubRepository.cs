using Microsoft.EntityFrameworkCore;
using PClub.Application.Clubs;
using PClub.Infrastructure.Persistence;
using PClub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PClub.Infrastructure.Clubs
{
    /// <summary>
    /// Клубы из PostgreSQL.
    /// </summary>
    public sealed class EfClubRepository : IClubRepository
    {

        private readonly AppDbContext _db;
        /// <summary>
        /// Создаёт репозиторий.
        /// </summary>
        public EfClubRepository(AppDbContext db) => _db = db;
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
    }
}
