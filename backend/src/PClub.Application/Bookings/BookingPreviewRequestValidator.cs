using FluentValidation;
using PClub.Domain.Entities;

namespace PClub.Application.Bookings
{
    /// <summary>
    /// Проверка формы запроса предпросчёта.
    /// </summary>
    public sealed class BookingPreviewRequestValidator
        : AbstractValidator<BookingPreviewRequest>
    {
        /// <summary>
        /// Задаёт правила.
        /// </summary>
        public BookingPreviewRequestValidator()
        {
            RuleFor(x => x.ZoneId)
                .NotEmpty().WithMessage("Зона обязательна.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Начало обязательно.");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("Окончание должно быть позже начала.");

            // Правило о длительности смотрит на два поля, но объявлено на
            // EndTime: тогда имя поля попадёт в ответ, а не пустой ключ "".
            RuleFor(x => x.EndTime)
                .Must((request, endTime) =>
                    endTime - request.StartTime <= Booking.MaxDuration)
                .WithMessage($"Бронь не может быть длиннее {Booking.MaxDuration.TotalHours} часов.")
                .When(x => x.EndTime > x.StartTime);
        }
    }
}
