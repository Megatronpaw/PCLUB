using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using PClub.Application.Abstractions;

namespace PClub.Infrastructure.Persistence
{
    /// <summary>
    /// Фиксация изменений через EF Core.
    /// </summary>
    public sealed class UnitOfWork : IUnitOfWork
    {
        /// <summary>Сколько раз повторять при конфликте сериализации.</summary>
        private const int MaxRetryAttempts = 5;

        /// <summary>Базовая задержка перед повтором, миллисекунды.</summary>
        private const int BaseRetryDelayMs = 40;

        /// <summary>Конфликт сериализации.</summary>
        private const string SerializationFailure = "40001";

        /// <summary>Взаимная блокировка.</summary>
        private const string DeadlockDetected = "40P01";

        private readonly AppDbContext _db;
        private readonly ILogger<UnitOfWork> _logger;

        /// <summary>
        /// Создаёт единицу работы.
        /// </summary>
        /// <param name="db">Контекст базы.</param>
        /// <param name="logger">Журнал: сюда пишутся повторы.</param>
        public UnitOfWork(AppDbContext db, ILogger<UnitOfWork> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                var translated = PostgresErrorTranslator.Translate(exception);

                if (translated is not null)
                {
                    throw translated;
                }

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                for (var attempt = 1; ; attempt++)
                {
                    try
                    {
                        await using var transaction = await _db.Database.BeginTransactionAsync(
                            IsolationLevel.Serializable, cancellationToken);

                        var result = await operation(cancellationToken);

                        await transaction.CommitAsync(cancellationToken);

                        return result;
                    }
                    catch (Exception exception)
                        when (IsTransient(exception) && attempt < MaxRetryAttempts)
                    {
                        _db.ChangeTracker.Clear();

                        var delay = BaseRetryDelayMs * (1 << (attempt - 1));
                        var jitter = Random.Shared.Next(0, delay / 2 + 1);

                        _logger.LogWarning(
                            "Конфликт сериализации, попытка {Attempt} из {Max}. Пауза {Delay} мс.",
                            attempt, MaxRetryAttempts, delay + jitter);

                        await Task.Delay(delay + jitter, cancellationToken);
                    }
                }
            });
        }

        /// <summary>Можно ли повторить эту ошибку.</summary>
        private static bool IsTransient(Exception exception)
        {
            var postgres = exception as PostgresException
                ?? exception.InnerException as PostgresException;

            return postgres?.SqlState is SerializationFailure or DeadlockDetected;
        }
    }
}
