using PClub.Application.Common;
using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Application.Clubs
{
    /// <summary>
    /// Операции над клубами.
    /// </summary>
    public interface IClubService
    {
        /// <summary>
        /// Все клубы.
        /// </summary>
        Task<IReadOnlyList<ClubDto>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Один клуб.
        /// </summary>
        ///<exception cref="Domain.Exceptions.NotFoundException">Клуба нет</exception>
        Task<ClubDto> GetByIdAsync(Guid id, CancellationToken cancellationToken= default);

        /// <summary>
        /// Меняет состояние клуба в каталоге.
        /// </summary>
        ///<exception cref="Domain.Exceptions.NotFoundException">Клуба нет</exception>
        ///<exception cref="Domain.Exceptions.ConflictException">Публикация клуба без мест</exception>
        Task<ClubDto> ChangeStatusAsync(
            Guid id, ClubStatus status, CancellationToken cancellationToken= default);

        /// <summary>
        /// Страница каталога
        /// </summary>
        Task<PagedResult<ClubListItemDto>> SearchAsync(
            ClubCatalogQuery query, CancellationToken cancellationToken= default);
    }
}
