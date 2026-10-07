using Microsoft.AspNetCore.Mvc;
using PClub.Application.Bookings;
using PClub.Application.Clubs;
using PClub.Domain;
using PClub.Domain.Entities;

namespace PClub.Api.Controllers
{
    /// <summary>
    /// Брони.
    /// </summary>
    public sealed class BookingsController : ApiControllerBase
    {
        private readonly IClubRepository _clubs;

        /// <summary>Создаёт контроллер.</summary>
        /// <param name="clubs">Доступ к клубам.</param>
        public BookingsController(IClubRepository clubs) => _clubs = clubs;

        /// <summary>
        /// Считает стоимость брони, ничего не создавая.
        /// </summary>
        /// <param name="request">Зона и интервал.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Стоимость посчитана.</response>
        /// <response code="400">Интервал не соответствует правилам.</response>
        /// <response code="404">Зоны с таким идентификатором нет.</response>
        /// <response code="409">Интервал выходит за часы работы клуба.</response>
        [HttpPost("preview")]
        public async Task<ActionResult<BookingPreviewResponse>> Preview(
            [FromBody] BookingPreviewRequest request,
            CancellationToken cancellationToken)
        {
            var zone = await _clubs.GetZoneByIdAsync(request.ZoneId, cancellationToken);

            if (zone is null)
            {
                return NotFound($"Зона '{request.ZoneId}' не найдена");
            }

            Booking.ValidateRange(request.StartTime, request.EndTime);

            var club = await _clubs.GetClubByZoneIdAsync(request.ZoneId, cancellationToken);

            if (club is not null
                && !club.IsWithinOpeningHours(request.StartTime, request.EndTime))
            {
                return Conflict(
                    $"Клуб работает с {club.OpeningTime:HH\\:mm} до {club.ClosingTime:HH\\:mm}");
            }

            var total = Booking.CalculatePrice(
                zone.PricePerHourCents, request.StartTime, request.EndTime);

            return Ok(new BookingPreviewResponse(
                zone.Id,
                zone.Name,
                (request.EndTime - request.StartTime).TotalHours,
                zone.PricePerHourCents,
                total,
                Money.Format(total)));
        }
    }
}
