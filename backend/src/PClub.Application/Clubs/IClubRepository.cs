using PClub.Domain.Entities;

namespace PClub.Application.Clubs
{
    /// <summary>
    /// Доступ к клубам.
    /// </summary>
    public interface IClubRepository
    {
        /// <summary>Все клубы со зонами и местами.</summary>
        /// <param name="cancellation">Токен отмены.</param>
        Task<IReadOnlyList<Club>> GetAllAsync(CancellationToken cancellation = default);

        /// <summary>Клуб по идентификатору, или null.</summary>
        /// <param name="id">Идентификатор клуба.</param>
        /// <param name="cancellation">Токен отмены.</param>
        Task<Club?> GetByIdAsync(Guid id, CancellationToken cancellation = default);

        /// <summary>
        /// Зона по идентификатору, или null. Ищет во всех клубах.
        /// </summary>
        /// <param name="zoneId">Идентификатор зоны.</param>
        /// <param name="cancellation">Токен отмены.</param>
        /// <returns>Зона или <c>null</c>, если такой нет.</returns>
        Task<Zone?> GetZoneByIdAsync(Guid zoneId, CancellationToken cancellation = default);

        /// <summary>
        /// Клуб, которому принадлежит зона, или null.
        /// </summary>
        /// <param name="zoneId">Идентификатор зоны.</param>
        /// <param name="cancellation">Токен отмены.</param>
        /// <returns>Клуб или <c>null</c>, если зоны нет.</returns>
        Task<Club?> GetClubByZoneIdAsync(Guid zoneId, CancellationToken cancellation = default);

        /// <summary>
        /// Зона для изменения — отслеживаемая, с местами.
        /// </summary>
        /// <param name="zoneId">Идентификатор зоны.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Зона или <c>null</c>, если такой нет.</returns>
        Task<Zone?> GetTrackedZoneAsync(Guid zoneId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Клуб для изменения — со зонами и местами, отслеживаемый.
        /// </summary>
        /// <param name="id">Идентификатор клуба.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Клуб или <c>null</c>, если такого нет.</returns>
        Task<Club?> GetTrackedAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Место вместе с зоной и клубом.</summary>
        /// <param name="seatId">Идентификатор места.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Место или <c>null</c>, если такого нет.</returns>
        Task<Seat?> GetSeatWithContextAsync(
            Guid seatId, CancellationToken cancellationToken = default);
    }
}
