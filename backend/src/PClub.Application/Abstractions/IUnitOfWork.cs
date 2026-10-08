namespace PClub.Application.Abstractions
{
    /// <summary>
    /// Фиксация изменений и управление транзакциями.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Отправляет накопленные изменения в базу.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Сколько строк затронуто.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполняет операцию в транзакции уровня Serializable, повторяя её
        /// при конфликте сериализации или взаимной блокировке.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="operation">Что сделать. Может быть вызвано несколько раз.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Результат операции.</returns>
        /// <remarks>
        /// ВАЖНО: операция может выполниться повторно. Внутри допустима только
        /// работа с базой — ничего необратимого вроде отправки письма.
        /// </remarks>
        Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default);
    }
}
