using Microsoft.EntityFrameworkCore;
using PClub.Domain.Entities;
using PClub.Domain.Enums;

namespace PClub.Infrastructure.Persistence
{
    /// <summary>
    /// Демонстрационные данные для разработки.
    /// </summary>
    public sealed class DatabaseSeeder
    {
        /// <summary>
        /// Города, названия и базовые ставки: из них собирается разнообразный
        /// каталог, на котором видно работу фильтров главы 09.
        /// </summary>
        private static readonly (string City, string Suffix, long BasePrice)[] Templates =
        [
            ("Москва", "Арбат", 18000),
            ("Москва", "Сокол", 15000),
            ("Москва", "Кузьминки", 9000),
            ("Санкт-Петербург", "Невский", 16000),
            ("Санкт-Петербург", "Приморский", 11000),
            ("Казань", "Кремлёвская", 12000),
            ("Казань", "Горки", 7500),
            ("Новосибирск", "Центральный", 10000),
            ("Екатеринбург", "Плотинка", 13000),
            ("Краснодар", "Красная", 8500),
        ];

        private readonly AppDbContext _db;

        /// <summary>Создаёт сидер.</summary>
        /// <param name="db">Контекст базы.</param>
        public DatabaseSeeder(AppDbContext db) => _db = db;

        /// <summary>
        /// Заполняет базу, если она пуста.
        /// </summary>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <remarks>
        /// Проверка «есть ли хоть один клуб» делает метод идемпотентным:
        /// приложение перезапускается десятки раз за вечер, и без неё
        /// каждый запуск добавлял бы ещё один комплект демо-данных.
        /// </remarks>
        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            if (!await _db.Users.AnyAsync(cancellationToken))
            {
                _db.Users.AddRange(
                    new User("ivan@example.com", "Иван Петров"),
                    new User("maria@example.com", "Мария Сидорова"));

                await _db.SaveChangesAsync(cancellationToken);
            }

            if (await _db.Clubs.AnyAsync(cancellationToken))
            {
                return;
            }

            foreach (var (city, suffix, basePrice) in Templates)
            {
                var club = new Club(
                    name: $"PClub {suffix}",
                    city: city,
                    address: $"ул. {suffix}, 1",
                    openingTime: new TimeOnly(10, 0),
                    closingTime: new TimeOnly(23, 0));

                var standard = new Zone("Стандарт", basePrice);
                var vip = new Zone("VIP", basePrice * 2);

                club.AddZone(standard);
                club.AddZone(vip);

                for (var i = 1; i <= 8; i++)
                {
                    standard.AddSeat(new Seat($"PC-{i:00}"));
                }

                for (var i = 1; i <= 3; i++)
                {
                    vip.AddSeat(new Seat($"VIP-{i:00}"));
                }

                club.ChangeStatus(ClubStatus.Published, hasBookableSeats: true);

                _db.Clubs.Add(club);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
