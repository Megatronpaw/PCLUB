using FluentValidation;
using PClub.Domain.Entities;

namespace PClub.Application.Auth;

/// <summary>Проверка формы запроса регистрации.</summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>Минимальная длина пароля.</summary>
    private const int MinPasswordLength = 8;

    /// <summary>
    /// Максимальная длина пароля.
    /// </summary>
    /// <remarks>
    /// Не для «безопасности», а против отказа в обслуживании: PBKDF2 от
    /// строки в мегабайт займёт секунды процессорного времени на запрос.
    /// </remarks>
    private const int MaxPasswordLength = 128;

    /// <summary>Задаёт правила.</summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Почта обязательна.")
            .EmailAddress().WithMessage("Похоже, это не адрес почты.")
            .MaximumLength(256);

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Имя обязательно.")
            .MaximumLength(User.MaxDisplayNameLength);

        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Пароль обязателен.")
            .MinimumLength(MinPasswordLength)
            .WithMessage($"Пароль короче {MinPasswordLength} символов.")
            .MaximumLength(MaxPasswordLength)
            .Must(p => p.Any(char.IsDigit))
            .WithMessage("В пароле должна быть хотя бы одна цифра.")
            .Must(p => p.Any(char.IsLetter))
            .WithMessage("В пароле должна быть хотя бы одна буква.");
    }
}
