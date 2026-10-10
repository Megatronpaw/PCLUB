using FluentValidation;
using PClub.Application.Abstractions;
using PClub.Domain.Entities;

namespace PClub.Application.Bookings
{
    /// <summary>
    /// Проверка формы запроса создания брони.
    /// </summary>
    public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        /// <summary>
        /// Задаёт правила.
        /// </summary>
        /// <param name="clock">Часы: нужны для правила «не в прошлом».</param>
        public CreateBookingRequestValidator(IClock clock)
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("Пользователь обязателен.");
            RuleFor(x => x.SeatId).NotEmpty().WithMessage("Место обязательно.");

            RuleFor(x => x.StartTime)
                .GreaterThan(clock.UtcNow).WithMessage("Начало брони уже в прошлом.");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("Окончание должно быть позже начала.");

            RuleFor(x => x.EndTime)
                .Must((request, endTime) =>
                    endTime - request.StartTime <= Booking.MaxDuration)
                .WithMessage($"Бронь не может быть длиннее {Booking.MaxDuration.TotalHours} часов.")
                .When(x => x.EndTime > x.StartTime);
        }
    }
}
