using FluentValidation;
using PClub.Domain.Enums;

namespace PClub.Application.Clubs
{
    /// <summary>
    /// Проверка формы запроса смены состояния клуба.
    /// </summary>
    /// <remarks>
    /// JsonStringEnumConverter ловит неизвестные строки сам, но число пропускает:
    /// перечисление в C# — это обёртка над int, и (ClubStatus)99 для компилятора
    /// совершенно законно. Поэтому проверку нужно писать руками.
    /// </remarks>
    public sealed class ChangeClubStatusRequestValidator
        : AbstractValidator<ChangeClubStatusRequest>
    {
        /// <summary>
        /// Задаёт правила.
        /// </summary>
        public ChangeClubStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .Must(status => Enum.IsDefined(status))
                .WithMessage(
                    $"Допустимые состояния: {string.Join(", ", Enum.GetNames<ClubStatus>())}.");
        }
    }
}
