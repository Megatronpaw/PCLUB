namespace PClub.Domain.Exceptions
{
    /// <summary>
    /// Основа всех исключений предметной области.
    /// </summary>
    /// <remarks>
    /// Общий предок нужен, чтобы в главе 08 один обработчик мог отличить
    /// «так нельзя по правилам» от настоящей поломки сервера и ответить
    /// 4xx вместо 500.
    ///
    /// Текст таких исключений показывается пользователю, поэтому он пишется
    /// по-человечески и без внутренних подробностей.
    /// </remarks>
    public abstract class DomainException : Exception
    {
        /// <summary>Создаёт исключение с сообщением.</summary>
        /// <param name="message">Текст для пользователя.</param>
        protected DomainException(string message) : base(message)
        {
        }
    }
}
