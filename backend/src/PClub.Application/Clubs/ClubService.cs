using PClub.Application.Abstractions;
using PClub.Application.Common;
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

        ///<inheritdoc/>
        public Task<PagedResult<ClubListItemDto>> SearchAsync(
            ClubCatalogQuery query, CancellationToken cancellationToken = default)
        {
            ValidatePriceRange(query);

            return _clubs.SearchAsync(query, cancellationToken);
        }

        /// <summary>
        /// Проверяет границы цены до обращения к базе.
        /// </summary>
        /// <param name="query">Параметры запроса каталога.</param>
        /// <exception cref="DomainValidationException">Границы заданы бессмысленно.</exception>
        /// <remarks>
        /// Репозиторий получает только осмысленные значения и не разбирается,
        /// что делать с отрицательной ценой или перевёрнутым диапазоном.
        /// </remarks>
        private static void ValidatePriceRange(ClubCatalogQuery query)
        {
            if (query.MinPricePerHourCents < 0 || query.MaxPricePerHourCents < 0)
            {
                throw new DomainValidationException("Цена не может быть отрицательной.");
            }

            if (query.MinPricePerHourCents is { } min
                && query.MaxPricePerHourCents is { } max
                && min > max)
            {
                throw new DomainValidationException(
                    "Нижняя граница цены больше верхней: поменяй их местами.");
            }
        }
    }
}
