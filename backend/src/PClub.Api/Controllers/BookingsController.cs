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

       
        private readonly IBookingService _bookings;

        /// <summary>Создаёт контроллер.</summary>
        /// <param name="clubs">Доступ к клубам.</param>
        /// <param name="bookings">Операции над бронями.</param>
        public BookingsController(IClubRepository clubs, IBookingService bookings)
        {
            _clubs = clubs;
            _bookings = bookings;
        }

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

        /// <summary>Создаёт бронь.</summary>
        /// <param name="request">Место и время.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="201">Бронь создана.</response>
        /// <response code="400">Данные не прошли проверку.</response>
        /// <response code="404">Места нет.</response>
        /// <response code="409">Место занято или недоступно.</response>
        [HttpPost]
        public async Task<ActionResult<BookingDto>> Create(
            [FromBody] CreateBookingRequest request,
            CancellationToken cancellationToken)
        {
            var booking = await _bookings.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        /// <summary>Возвращает бронь.</summary>
        /// <param name="id">Идентификатор брони.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Бронь найдена.</response>
        /// <response code="404">Брони нет.</response>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookingDto>> GetById(
            Guid id, CancellationToken cancellationToken) =>
            Ok(await _bookings.GetByIdAsync(id, cancellationToken));

        /// <summary>Отменяет бронь.</summary>
        /// <param name="id">Идентификатор брони.</param>
        /// <param name="request">Кто отменяет.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Бронь отменена.</response>
        /// <response code="403">Бронь принадлежит другому пользователю.</response>
        /// <response code="404">Брони нет.</response>
        /// <response code="409">Окно отмены закрыто или бронь уже не активна.</response>
        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<BookingDto>> Cancel(
            Guid id,
            [FromBody] CancelBookingRequest request,
            CancellationToken cancellationToken) =>
            Ok(await _bookings.CancelAsync(id, request.UserId, cancellationToken));

        /// <summary>Занятые интервалы места на дату.</summary>
        /// <param name="query">Место и дата.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Занятость.</response>
        /// <response code="400">Параметры не прошли проверку.</response>
        /// <response code="404">Места нет.</response>
        [HttpGet("availability")]
        public async Task<ActionResult<SeatAvailabilityDto>> GetAvailability(
            [FromQuery] SeatAvailabilityQuery query,
            CancellationToken cancellationToken) =>
            Ok(await _bookings.GetAvailabilityAsync(
                query.SeatId, query.Date, cancellationToken));
    }
}
