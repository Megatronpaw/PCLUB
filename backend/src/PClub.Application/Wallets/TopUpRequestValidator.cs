using FluentValidation;
using PClub.Domain.Entities;

namespace PClub.Application.Wallets;

/// <summary>Проверка формы запроса пополнения.</summary>
public sealed class TopUpRequestValidator : AbstractValidator<TopUpRequest>
{
    /// <summary>Минимальное пополнение: 10 рублей.</summary>
    private const long MinTopUpCents = 1000;

    /// <summary>Задаёт правила.</summary>
    public TopUpRequestValidator()
    {

        RuleFor(x => x.AmountCents)
            .GreaterThanOrEqualTo(MinTopUpCents)
            .WithMessage($"Минимальное пополнение — {MinTopUpCents / 100} руб.")
            .LessThanOrEqualTo(User.MaxTopUpCents)
            .WithMessage($"Максимальное пополнение — {User.MaxTopUpCents / 100} руб.");
    }
}
