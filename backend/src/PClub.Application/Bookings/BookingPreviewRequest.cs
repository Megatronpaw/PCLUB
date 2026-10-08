namespace PClub.Application.Bookings
{
    /// <summary>
    /// Запрос предпросчёта стоимости брони.
    /// </summary>
    /// <param name="ZoneId">Зона, по ставке которой считаем.</param>
    /// <param name="StartTime">Начало, UTC. Должно быть на границе получаса.</param>
    /// <param name="EndTime">Конец, UTC.</param>
    public sealed record BookingPreviewRequest(
        Guid ZoneId, DateTimeOffset StartTime, DateTimeOffset EndTime);

    /// <summary>
    /// Результат предпросчёта.
    /// </summary>
    /// <param name="ZoneId">Зона, по которой считали.</param>
    /// <param name="ZoneName">Название зоны.</param>
    /// <param name="Hours">Длительность в часах.</param>
    /// <param name="PricePerHourCents">Ставка зоны за час, копейки.</param>
    /// <param name="TotalPriceCents">Итого к оплате, копейки.</param>
    /// <param name="TotalPriceFormatted">Итого в виде, пригодном для показа человеку.</param>
    public sealed record BookingPreviewResponse(
        Guid ZoneId,
        string ZoneName,
        double Hours,
        long PricePerHourCents,
        long TotalPriceCents,
        string TotalPriceFormatted);
}
