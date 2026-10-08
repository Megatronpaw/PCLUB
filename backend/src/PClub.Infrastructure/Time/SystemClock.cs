using PClub.Application.Abstractions;

namespace PClub.Infrastructure.Time
{
    /// <summary>
    /// Настоящие часы. Singletion: состояния нет.
    /// </summary>
    public sealed class SystemClock : IClock
    {
        ///<inheritdoc/>
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
