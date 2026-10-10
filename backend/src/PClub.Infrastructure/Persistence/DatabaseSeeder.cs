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
        /// Города, названия, базовые ставки и описания: из них собирается
        /// разнообразный каталог, на котором видна работа фильтров.
        /// </summary>
        private static readonly (string City, string Suffix, long BasePrice, string Description)[] Templates =
        [
            ("Москва", "Арбат", 18000,
                "Флагманский зал в центре: топовые сборки, профессиональная периферия и бар."),
            ("Москва", "Сокол", 15000,
                "Тихий клуб рядом с метро. Подходит для долгих сессий и работы."),
            ("Москва", "Кузьминки", 9000,
                "Бюджетный зал у дома: всё нужное для сетевых игр без переплаты."),
            ("Санкт-Петербург", "Невский", 16000,
                "Два этажа на главной улице города, отдельная зона для турниров."),
            ("Санкт-Петербург", "Приморский", 11000,
                "Спокойный район, большие мониторы и кресла с поддержкой поясницы."),
            ("Казань", "Кремлёвская", 12000,
                "Светлый зал в двух шагах от кремля, кофе и настольные игры в перерывах."),
            ("Казань", "Горки", 7500,
                "Самые доступные цены в городе и круглосуточные выходные."),
            ("Новосибирск", "Центральный", 10000,
                "Крупнейший зал за Уралом: регулярные турниры и своя команда."),
            ("Екатеринбург", "Плотинка", 13000,
                "Клуб у набережной с панорамными окнами и зоной отдыха."),
            ("Краснодар", "Красная", 8500,
                "Южный филиал сети: кондиционеры, прохладные напитки и быстрый интернет."),
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

            foreach (var (city, suffix, basePrice, description) in Templates)
            {
                var club = new Club(
                    name: $"PClub {suffix}",
                    city: city,
                    address: $"ул. {suffix}, 1",
                    openingTime: new TimeOnly(10, 0),
                    closingTime: new TimeOnly(23, 0),
                    description: description);

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

                var underRepair = new Seat("PC-09");
                underRepair.SendToMaintenance();
                standard.AddSeat(underRepair);

                club.ChangeStatus(ClubStatus.Published, hasBookableSeats: true);

                _db.Clubs.Add(club);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
