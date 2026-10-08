using PClub.Application.Abstractions;
using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Application.Clubs
{


    /// <summary>
    /// Операции над клубами.
    /// </summary>
    public sealed class ClubService : IClubService
    {
        private readonly IClubRepository _clubs;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Создаёт сервис.
        /// </summary>
        public ClubService(IClubRepository clubs, IUnitOfWork unitOfWork)
        {
            _clubs = clubs;
            _unitOfWork = unitOfWork;
        }

        ///<inheritdoc/>
        public async Task<IReadOnlyList<ClubDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var clubs = await _clubs.GetAllAsync(cancellationToken);

            return clubs.Select(ClubDto.From).ToList();
        }

        ///<inheritdoc/>
        public async Task<ClubDto> GetByIdAsync(
            Guid id, CancellationToken cancellationToken = default)
        {
            // ?? throw вместо if (club is null) return NotFound():
            // сервис не знает про HTTP, он бросает исключение предметной
            // области. Превращать его в 404 будет обработчик из главы 08.
            var club = await _clubs.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Club", id);

            return ClubDto.From(club);
        }

        ///<inheritdoc/>
        public async Task<ClubDto> ChangeStatusAsync(
            Guid id, ClubStatus status, CancellationToken cancellationToken = default)
        {
            var club = await _clubs.GetTrackedAsync(id, cancellationToken)
                ?? throw new NotFoundException("Club", id);

            var hasBookableSeats = club.Zones.Any(z => z.ActiveSeatsCount > 0);

            club.ChangeStatus(status, hasBookableSeats);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ClubDto.From(club);
        }
    }
}
