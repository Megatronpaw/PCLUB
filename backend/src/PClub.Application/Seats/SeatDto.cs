using PClub.Domain.Entities;
using PClub.Domain.Enums;

namespace PClub.Application.Seats
{
    /// <summary>
    /// Место, как его видит клиент API.
    /// </summary>
    /// <param name="Id">Идентификатор места.</param>
    /// <param name="Label">Метка на корпусе, например PC-01.</param>
    /// <param name="Specs">Железо места; null означает «как в зоне».</param>
    /// <param name="Status">Текущее состояние.</param>
    public sealed record SeatDto(Guid Id, string Label, string? Specs, SeatStatus Status)
    {
        /// <summary>Собирает DTO из сущности.</summary>
        /// <param name="seat">Место предметной области.</param>
        public static SeatDto From(Seat seat) =>
            new(seat.Id, seat.Label, seat.Specs, seat.Status);
    }
}
