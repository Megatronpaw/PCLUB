using PClub.Application.Seats;
using PClub.Domain.Entities;
using PClub.Domain.Enums;

namespace PClub.Application.Clubs
{
    /// <summary>
    /// Клуб, как его видит клиент API.
    /// </summary>
    /// <param name="Id">Идентификатор.</param>
    /// <param name="Name">Название.</param>
    /// <param name="City">Город.</param>
    /// <param name="Address">Адрес.</param>
    /// <param name="Description">Описание.</param>
    /// <param name="OpeningTime">Время открытия в виде «HH:mm».</param>
    /// <param name="ClosingTime">Время закрытия в виде «HH:mm».</param>
    /// <param name="IsOpenAllDay">Работает ли клуб круглосуточно.</param>
    /// <param name="Status">Состояние в каталоге.</param>
    /// <param name="Zones">Зоны клуба.</param>
    public sealed record ClubDto(
        Guid Id,
        string Name,
        string City,
        string Address,
        string Description,
        string OpeningTime,
        string ClosingTime,
        bool IsOpenAllDay,
        ClubStatus Status,
        IReadOnlyList<ZoneDto> Zones)
    {
        /// <summary>Собирает DTO из сущности.</summary>
        /// <param name="club">Клуб предметной области.</param>
        /// <remarks>
        /// Время отдаётся строкой «HH:mm», а не объектом: TimeOnly
        /// сериализуется в JSON неудобным для клиента видом.
        /// </remarks>
        public static ClubDto From(Club club) => new(
            club.Id,
            club.Name,
            club.City,
            club.Address,
            club.Description,
            club.OpeningTime.ToString("HH:mm"),
            club.ClosingTime.ToString("HH:mm"),
            club.IsOpenAllDay,
            club.Status,
            club.Zones.Select(ZoneDto.From).ToList());
    }

    /// <summary>
    /// Зона, как её видит клиент API.
    /// </summary>
    /// <param name="Id">Идентификатор.</param>
    /// <param name="Name">Название зоны.</param>
    /// <param name="PricePerHourCents">Ставка за час в копейках.</param>
    /// <param name="Specs">Описание железа зоны.</param>
    /// <param name="ActiveSeatsCount">Сколько мест доступно для брони.</param>
    /// <param name="Seats">Места зоны.</param>
    public sealed record ZoneDto(
        Guid Id,
        string Name,
        long PricePerHourCents,
        string Specs,
        int ActiveSeatsCount,
        IReadOnlyList<SeatDto> Seats)
    {
        /// <summary>Собирает DTO из сущности.</summary>
        /// <param name="zone">Зона предметной области.</param>
        public static ZoneDto From(Zone zone) => new(
            zone.Id,
            zone.Name,
            zone.PricePerHourCents,
            zone.Specs,
            zone.ActiveSeatsCount,
            zone.Seats.Select(SeatDto.From).ToList());
    }
}
