using System;
using System.Collections.Generic;
using System.Text;
using PClub.Application.Abstractions;

namespace PClub.Infrastructure.Persistence
{
    /// <summary>
    /// Фиксация изменений через EF Core.
    /// </summary>
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Создаёт единицу работы.
        /// </summary>
        /// <param name="db">Контекст базы.</param>
        public UnitOfWork(AppDbContext db) => _db = db;

        ///<inheritdoc/>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _db.SaveChangesAsync(cancellationToken);
    }
}
