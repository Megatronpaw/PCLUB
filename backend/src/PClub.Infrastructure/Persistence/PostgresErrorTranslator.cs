using Microsoft.EntityFrameworkCore;
using Npgsql;
using PClub.Domain.Exceptions;

namespace PClub.Infrastructure.Persistence
{
    /// <summary>
    /// Переводит отказы PostgreSQL в исключения предметной области.
    /// </summary>
    /// <remarks>
    /// Здесь — единственное место в проекте, где код знает про номера ошибок
    /// PostgreSQL. Дальше по стеку идут уже наши исключения, и слой Application
    /// остаётся независимым от того, какая под ним база.
    /// </remarks>
    internal static class PostgresErrorTranslator
    {
        /// <summary>Нарушено ограничение исключения (пересечение диапазонов).</summary>
        private const string ExclusionViolation = "23P01";

        /// <summary>Нарушена уникальность.</summary>
        private const string UniqueViolation = "23505";

        /// <summary>Нарушена ссылочная целостность.</summary>
        private const string ForeignKeyViolation = "23503";

        /// <summary>Имя нашего ограничения на пересечение броней.</summary>
        private const string BookingOverlapConstraint = "bookings_no_overlap";

        /// <summary>
        /// Возвращает исключение предметной области, если отказ нам понятен.
        /// </summary>
        /// <param name="exception">Отказ, пойманный при сохранении.</param>
        /// <returns>Исключение или <c>null</c>, если случай не наш.</returns>
        public static DomainException? Translate(DbUpdateException exception)
        {
            // Настоящая причина лежит во внутреннем исключении драйвера.
            if (exception.InnerException is not PostgresException postgres)
            {
                return null;
            }

            return postgres.SqlState switch
            {
                // Главный случай главы: гонка проскочила проверку в сервисе,
                // и место спасло ограничение EXCLUDE.
                ExclusionViolation when postgres.ConstraintName == BookingOverlapConstraint =>
                    new ConflictException(
                        "Место уже занято в выбранное время. Выберите другое время или место."),

                // Сообщение по имени ограничения: пользователю не нужно знать,
                // что сломалось внутри, но текст должен подсказывать, что делать.
                UniqueViolation when postgres.ConstraintName?.Contains("email") == true =>
                    new ConflictException("Пользователь с такой почтой уже есть."),

                ExclusionViolation or UniqueViolation =>
                    new ConflictException("Такая запись уже существует."),

                // Ссылка на то, чего нет: до главы 11 userId присылает клиент,
                // так что это самая вероятная плохая просьба к этому эндпоинту.
                ForeignKeyViolation =>
                    new DomainValidationException(
                        "Запрос ссылается на несуществующую запись: проверь userId и seatId."),

                // Всё остальное — не наше дело. Пусть летит выше как 500:
                // это настоящая поломка, и её надо увидеть в логах со стеком.
                _ => null,
            };
        }
    }
}
