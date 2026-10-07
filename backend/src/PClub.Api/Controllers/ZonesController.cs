using Microsoft.AspNetCore.Mvc;
using PClub.Application.Clubs;
using PClub.Application.Zones;

namespace PClub.Api.Controllers
{
    /// <summary>
    /// Зоны клубов.
    /// </summary>
    public sealed class ZonesController : ApiControllerBase
    {
        private readonly IZoneService _zones;

        /// <summary>
        /// Создаёт контроллер.
        /// </summary>
        /// <param name="zones">Операции над зонами.</param>
        public ZonesController(IZoneService zones) => _zones = zones;

        /// <summary>
        /// Меняет ставку зоны за час.
        /// </summary>
        /// <param name="id">Идентификатор зоны.</param>
        /// <param name="request">Новая ставка.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Ставка изменена.</response>
        /// <response code="400">Ставка не положительная.</response>
        /// <response code="404">Зоны с таким идентификатором нет.</response>
        [HttpPut("{id:guid}/price")]
        public async Task<ActionResult<ZoneDto>> ChangePrice(
            Guid id,
            [FromBody] ChangeZonePriceRequest request,
            CancellationToken cancellationToken) =>
            Ok(await _zones.ChangePriceAsync(
                id, request.PricePerHourCents, cancellationToken));
    }

    /// <summary>
    /// Запрос смены ставки зоны.
    /// </summary>
    /// <param name="PricePerHourCents">Новая ставка за час в копейках.</param>
    public sealed record ChangeZonePriceRequest(long PricePerHourCents) : IHasZonePrice;
}
