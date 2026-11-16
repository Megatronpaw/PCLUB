using PClub.Application.Abstractions;
using PClub.Application.Clubs;
using PClub.Domain.Enums;
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
        private readonly ICurrentUser _currentUser;

        /// <summary>
        /// Создаёт сервис.
        /// </summary>
        /// <param name="clubs">Доступ к клубам и зонам.</param>
        /// <param name="currentUser">Текущий пользователь.</param>
        /// <param name="unitOfWork">Фиксация изменений.</param>
        public ZoneService(IClubRepository clubs, IUnitOfWork unitOfWork, ICurrentUser currentUser)
        {
            _clubs = clubs;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        /// <inheritdoc />
        public async Task<ZoneDto> ChangePriceAsync(
            Guid zoneId, long pricePerHourCents, CancellationToken cancellationToken = default)
        {
            var zone = await _clubs.GetTrackedZoneAsync(zoneId, cancellationToken)
                ?? throw new NotFoundException("Zone", zoneId);

            var club = await _clubs.GetClubByZoneIdAsync(zoneId, cancellationToken)
                ?? throw new NotFoundException("Zone", zoneId);
            _currentUser.RequireUserId();
            if (_currentUser.Role != UserRole.Admin
                && (_currentUser.Role != UserRole.Owner || club.OwnerId != _currentUser.UserId))
                throw new ForbiddenException("Вы не владелец этого клуба.");

            zone.ChangePrice(pricePerHourCents);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ZoneDto.From(zone);
        }
    }
}
