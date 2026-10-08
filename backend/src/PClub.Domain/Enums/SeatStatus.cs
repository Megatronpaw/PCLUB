
namespace PClub.Domain.Enums
{
    /// <summary>
    /// Состояние места: можно ли его бронировать.
    /// </summary>
    public enum SeatStatus
    {
        /// <summary>
        /// Работает, бронировать можно.
        /// </summary>
        Active = 0,
        ///<summary>На обслуживании: числиться, но бронировать нельзя.</summary>
        Maintenance = 1,
    }
}
