using PClub.Application.Clubs;

namespace PClub.Application.Zones
{
    /// <summary>
    /// Операции над зонами.
    /// </summary>
    public interface IZoneService
    {
        /// <summary>
        /// Меняет ставку зоны за час.
        /// </summary>
        /// <param name="zoneId">Идентификатор зоны.</param>
        /// <param name="pricePerHourCents">Новая ставка в копейках, больше нуля.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Зона после изменения.</returns>
        /// <exception cref="Domain.Exceptions.NotFoundException">Зоны с таким идентификатором нет.</exception>
        /// <exception cref="Domain.Exceptions.DomainValidationException">Ставка не положительная.</exception>
        Task<ZoneDto> ChangePriceAsync(
            Guid zoneId, long pricePerHourCents, CancellationToken cancellationToken = default);
    }
}
