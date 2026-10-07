using System;
using System.Collections.Generic;
using System.Text;

namespace PClub.Application.Abstractions
{
    /// <summary>
    /// Фиксация изменений.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Отправвляет накопленные изменения в базу.
        /// </summary>
        /// <returns>Сколько затронуто</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellation = default);
    }
}
