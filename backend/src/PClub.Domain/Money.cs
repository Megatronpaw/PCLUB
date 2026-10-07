namespace PClub.Domain
{
    /// <summary>
    /// Работа с деньгами. Все суммы — целые копейки.
    /// </summary>
    /// <remarks>
    /// Никаких double и float: 0.1 + 0.2 в них не равно 0.3, и на деньгах
    /// погрешность накапливается до того, что сверка перестаёт сходиться.
    /// </remarks>
    public static class Money
    {
        /// <summary>
        /// Копеек в рубле.
        /// </summary>
        public const long CentsPerUnit = 100;

        /// <summary>
        /// Считает цену за интервал по часовой ставке.
        /// </summary>
        /// <param name="pricePerHourCents">Ставка за час в копейках.</param>
        /// <param name="duration">Длительность.</param>
        /// <returns>Стоимость в копейках.</returns>
        /// <remarks>
        /// Умножение идёт ДО деления: при обратном порядке 90 минут дали бы
        /// 90 / 60 = 1 час, и полчаса потерялись бы молча.
        ///
        /// AwayFromZero, а не округление по умолчанию: .NET округляет к
        /// ближайшему чётному, и 2.5 стало бы 2, а 3.5 — 4. На счёте
        /// покупателю это выглядит произвольно.
        /// </remarks>
        public static long ForDuration(long pricePerHourCents, TimeSpan duration)
        {
            var minutes = (long)Math.Round(duration.TotalMinutes, MidpointRounding.AwayFromZero);

            return (long)Math.Round(
                pricePerHourCents * minutes / 60.0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Форматирует копейки для показа человеку: 20600 даёт «206 Р».
        /// </summary>
        /// <param name="cents">Сумма в копейках.</param>
        public static string Format(long cents) => $"{cents / CentsPerUnit} Р";
    }
}
