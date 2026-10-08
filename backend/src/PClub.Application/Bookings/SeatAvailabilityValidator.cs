using FluentValidation;
using PClub.Application.Abstractions;

namespace PClub.Application.Bookings
{
    /// <summary>
    /// Параметры запроса занятости. Приходят строкой запроса, поэтому
    /// свойства с init, а не позиционные параметры записи.
    /// </summary>
    public sealed record SeatAvailabilityQuery
    {
        /// <summary>Место.</summary>
        public Guid SeatId { get; init; }

        /// <summary>Дата.</summary>
        public DateOnly Date { get; init; }
    }

    /// <summary>
    /// Проверка параметров запроса занятости.
    /// </summary>
    public sealed class SeatAvailabilityValidator : AbstractValidator<SeatAvailabilityQuery>
    {
        /// <summary>
        /// Задаёт правила.
        /// </summary>
        /// <param name="clock">Часы: правило «не раньше сегодняшней» зависит от «сейчас».</param>
        public SeatAvailabilityValidator(IClock clock)
        {
            RuleFor(x => x.SeatId).NotEmpty().WithMessage("Место обязательно.");

            RuleFor(x => x.Date)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(clock.UtcNow.UtcDateTime))
                .WithMessage("Дата в прошлом: занятость уже не важна.");
        }
    }
}
