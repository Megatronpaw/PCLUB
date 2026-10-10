using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PClub.Application.Abstractions;
using PClub.Application.Bookings;
using PClub.Application.Clubs;
using PClub.Infrastructure.Bookings;
using PClub.Infrastructure.Clubs;
using PClub.Infrastructure.Persistence;
using PClub.Infrastructure.Time;

namespace PClub.Infrastructure
{
    /// <summary>
    /// Регистрация всего, что умеет слой инфраструктуры.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет базу, репозитории и прочие технические сервисы.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <param name="configuration">Конфигурация — оттуда строка подключения.</param>
        /// <param name="isDevelopment">Включать ли подробное логирование SQL.</param>
        /// <returns>Её же — чтобы вызовы можно было ставить цепочкой.</returns>
        /// <exception cref="InvalidOperationException">Строка подключения не задана.</exception>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            bool isDevelopment)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:Postgres не задан. Проверь конфигурацию.");

            services.AddDbContext<AppDbContext>(options =>
            {
                options
                    .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(3))
                    .UseSnakeCaseNamingConvention();

                if (isDevelopment)
                {
                    options
                        .EnableSensitiveDataLogging()
                        .LogTo(Console.WriteLine, LogLevel.Information);
                }
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IClubRepository, EfClubRepository>();
            services.AddScoped<IBookingRepository, EfBookingRepository>();

            services.AddScoped<DatabaseSeeder>();

            services.AddSingleton<IClock, SystemClock>();

            return services;
        }
    }
}
