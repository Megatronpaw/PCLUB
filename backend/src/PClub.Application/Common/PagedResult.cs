namespace PClub.Application.Common
{
    /// <summary>
    /// Одна страница выдачи.
    /// </summary>
    /// <typeparam name="T">Тип элемента.</typeparam>
    /// <param name="Items">Элементы этой страницы.</param>
    /// <param name="Page">Номер страницы, с единицы.</param>
    /// <param name="PageSize">Запрошенный размер страницы.</param>
    /// <param name="TotalCount">Сколько элементов всего подходит под фильтры.</param>
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount)
    {
        /// <summary>Сколько всего страниц.</summary>
        public int TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;

        /// <summary>Есть ли следующая страница.</summary>
        public bool HasNext => Page < TotalPages;

        /// <summary>Есть ли предыдущая страница.</summary>
        public bool HasPrevious => Page > 1;
    }
}
