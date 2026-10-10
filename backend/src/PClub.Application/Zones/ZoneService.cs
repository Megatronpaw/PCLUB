using PClub.Application.Abstractions;
using PClub.Application.Clubs;
using PClub.Domain.Exceptions;

namespace PClub.Application.Zones
{
    /// <summary>
    /// Операции над зонами.
    /// </summary>
    public sealed class ZoneService : IZoneService
    {
        private readonly IClubRepository _clubs;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Создаёт сервис.
        /// </summary>
        /// <param name="clubs">Доступ к клубам и зонам.</param>
        /// <param name="unitOfWork">Фиксация изменений.</param>
        public ZoneService(IClubRepository clubs, IUnitOfWork unitOfWork)
        {
            _clubs = clubs;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc />
        public async Task<ZoneDto> ChangePriceAsync(
            Guid zoneId, long pricePerHourCents, CancellationToken cancellationToken = default)
        {
            var zone = await _clubs.GetTrackedZoneAsync(zoneId, cancellationToken)
                ?? throw new NotFoundException("Zone", zoneId);

            zone.ChangePrice(pricePerHourCents);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ZoneDto.From(zone);
        }
    }
}
