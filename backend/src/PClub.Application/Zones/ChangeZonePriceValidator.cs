using FluentValidation;

namespace PClub.Application.Zones
{
    /// <summary>
    /// Проверка формы запроса смены ставки.
    /// </summary>
    /// <remarks>
    /// Валидатор живёт в Application, а запись запроса — в Api. Чтобы не тянуть
    /// ссылку из Application в Api (это сломало бы направление зависимостей),
    /// валидатор описан на интерфейсе: любой запрос со ставкой ему подойдёт.
    /// </remarks>
    public sealed class ChangeZonePriceValidator : AbstractValidator<IHasZonePrice>
    {
        /// <summary>
        /// Верхняя граница ставки: 100 000 рублей в час.
        /// </summary>
        /// <remarks>
        /// Нужна не для красоты: long вмещает 9 квинтиллионов, и такая ставка
        /// при умножении на длительность даст переполнение — цена станет
        /// отрицательной молча, без исключения.
        /// </remarks>
        private const long MaxPricePerHourCents = 10_000_000;

        /// <summary>
        /// Задаёт правила.
        /// </summary>
        public ChangeZonePriceValidator()
        {
            RuleFor(x => x.PricePerHourCents)
                .GreaterThan(0).WithMessage("Ставка должна быть больше нуля.")
                .LessThanOrEqualTo(MaxPricePerHourCents)
                .WithMessage("Ставка выглядит неправдоподобно большой.");
        }
    }

    /// <summary>
    /// Запрос, содержащий ставку зоны.
    /// </summary>
    public interface IHasZonePrice
    {
        /// <summary>
        /// Ставка за час в копейках.
        /// </summary>
        long PricePerHourCents { get; }
    }
}
