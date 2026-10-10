namespace PClub.Application.Clubs
{
    /// <summary>
    /// Клуб в списке каталога: только то, что нужно карточке.
    /// </summary>
    /// <param name="Id">Идентификатор.</param>
    /// <param name="Name">Название.</param>
    /// <param name="City">Город.</param>
    /// <param name="Address">Адрес.</param>
    /// <param name="MinPricePerHourCents">Минимальная ставка зоны, копейки.</param>
    /// <param name="MaxPricePerHourCents">Максимальная ставка зоны, копейки.</param>
    /// <param name="ZonesCount">Сколько зон.</param>
    /// <param name="SeatsCount">Сколько мест всего.</param>
    public sealed record ClubListItemDto(
        Guid Id,
        string Name,
        string City,
        string Address,
        long MinPricePerHourCents,
        long MaxPricePerHourCents,
        int ZonesCount,
        int SeatsCount);
}
