namespace PClub.Domain.Enums
{
    /// <summary>
    /// Состояние брони.
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>Подтверждена: оплачена, ещё не состоялась.</summary>
        Confirmed = 0,

        /// <summary>Отменена: деньги возвращены, место освобождено.</summary>
        Cancelled = 1,

        /// <summary>Завершена: время прошло.</summary>
        Completed = 2,
    }
}
