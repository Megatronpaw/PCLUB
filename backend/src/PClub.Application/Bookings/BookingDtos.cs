using PClub.Domain.Entities;
using PClub.Domain.Enums;

namespace PClub.Application.Bookings
{
    /// <summary>Запрос создания брони.</summary>
    /// <param name="UserId">Кто бронирует.</param>
    /// <param name="SeatId">Какое место.</param>
    /// <param name="StartTime">Начало.</param>
    /// <param name="EndTime">Окончание, в интервал не включается.</param>
    public sealed record CreateBookingRequest(
        Guid UserId,
        Guid SeatId,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime);

    /// <summary>Бронь в ответе.</summary>
    /// <param name="Id">Идентификатор.</param>
    /// <param name="UserId">Владелец.</param>
    /// <param name="ClubId">Клуб.</param>
    /// <param name="ZoneId">Зона.</param>
    /// <param name="SeatId">Место.</param>
    /// <param name="StartTime">Начало.</param>
    /// <param name="EndTime">Окончание.</param>
    /// <param name="Status">Состояние.</param>
    /// <param name="TotalPriceCents">Итого к оплате в копейках.</param>
    /// <param name="CreatedAt">Когда создана.</param>
    public sealed record BookingDto(
        Guid Id,
        Guid UserId,
        Guid ClubId,
        Guid ZoneId,
        Guid SeatId,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime,
        BookingStatus Status,
        long TotalPriceCents,
        DateTimeOffset CreatedAt)
    {
        /// <summary>Собирает DTO из сущности.</summary>
        /// <param name="booking">Бронь.</param>
        /// <returns>DTO для ответа.</returns>
        public static BookingDto From(Booking booking) => new(
            booking.Id,
            booking.UserId,
            booking.ClubId,
            booking.ZoneId,
            booking.SeatId,
            booking.StartTime,
            booking.EndTime,
            booking.Status,
            booking.TotalPriceCents,
            booking.CreatedAt);
    }

    /// <summary>Занятый интервал.</summary>
    /// <param name="StartTime">Начало.</param>
    /// <param name="EndTime">Окончание, не включается.</param>
    public sealed record BusyIntervalDto(DateTimeOffset StartTime, DateTimeOffset EndTime);

    /// <summary>Занятость места на дату.</summary>
    /// <param name="SeatId">Место.</param>
    /// <param name="Label">Метка места.</param>
    /// <param name="Date">Дата.</param>
    /// <param name="OpeningTime">Когда клуб открывается.</param>
    /// <param name="ClosingTime">Когда закрывается.</param>
    /// <param name="Busy">Занятые интервалы, по возрастанию начала.</param>
    public sealed record SeatAvailabilityDto(
        Guid SeatId,
        string Label,
        DateOnly Date,
        TimeOnly OpeningTime,
        TimeOnly ClosingTime,
        IReadOnlyList<BusyIntervalDto> Busy);

    /// <summary>Запрос отмены брони.</summary>
    /// <param name="UserId">Кто отменяет. До главы 11 приходит от клиента.</param>
    public sealed record CancelBookingRequest(Guid UserId);
}
