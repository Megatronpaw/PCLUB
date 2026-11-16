using Microsoft.EntityFrameworkCore;
using PClub.Application.Abstractions;
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
        private static readonly (string City, string Suffix, long BasePrice, int StandardSeats, string Description)[] Templates =
        [
            ("Москва", "Арбат", 18000, 14,
                "Флагманский зал в центре: топовые сборки, профессиональная периферия и бар."),
            ("Москва", "Сокол", 15000, 10,
                "Тихий клуб рядом с метро. Подходит для долгих сессий и работы."),
            ("Москва", "Кузьминки", 9000, 8,
                "Бюджетный зал у дома: всё нужное для сетевых игр без переплаты."),
            ("Санкт-Петербург", "Невский", 16000, 16,
                "Два этажа на главной улице города, отдельная зона для турниров."),
            ("Санкт-Петербург", "Приморский", 11000, 9,
                "Спокойный район, большие мониторы и кресла с поддержкой поясницы."),
            ("Казань", "Кремлёвская", 12000, 11,
                "Светлый зал в двух шагах от кремля, кофе и настольные игры в перерывах."),
            ("Казань", "Горки", 7500, 6,
                "Самые доступные цены в городе и круглосуточные выходные."),
            ("Новосибирск", "Центральный", 10000, 20,
                "Крупнейший зал за Уралом: регулярные турниры и своя команда."),
            ("Екатеринбург", "Плотинка", 13000, 12,
                "Клуб у набережной с панорамными окнами и зоной отдыха."),
            ("Краснодар", "Красная", 8500, 7,
                "Южный филиал сети: кондиционеры, прохладные напитки и быстрый интернет."),
        ];

        private readonly AppDbContext _db;

        /// <summary>Создаёт сидер.</summary>
        /// <param name="db">Контекст базы.</param>
        /// <param name="passwordHasher">Хеширование демо-паролей.</param>
        public DatabaseSeeder(AppDbContext db, IPasswordHasher passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }
        private readonly IPasswordHasher _passwordHasher;

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
            var demos = new[]
            {
                ("ivan@example.com", "Иван Петров", UserRole.User),
                ("maria@example.com", "Мария Сидорова", UserRole.User),
                ("owner@example.com", "Ольга Клубова", UserRole.Owner),
                ("admin@example.com", "Админ", UserRole.Admin)
            };
            foreach (var (email, name, role) in demos)
            {
                var existing = await _db.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
                if (existing is null)
                    _db.Users.Add(new User(email, name, _passwordHasher.Hash("Password123!"), role));
                else if (string.IsNullOrEmpty(existing.PasswordHash) && role == UserRole.User)
                    existing.ChangePasswordHash(_passwordHasher.Hash("Password123!"));
            }
            await _db.SaveChangesAsync(cancellationToken);
            var owner = await _db.Users.SingleAsync(u => u.Email == "owner@example.com", cancellationToken);
            if (owner.Role != UserRole.Owner)
                throw new InvalidOperationException("Демо-почта владельца занята пользователем без роли Owner.");
            var legacyOwner = Guid.Parse("00000000-0000-0000-0000-000000000011");
            var legacyClubs = await _db.Clubs.Where(c => c.OwnerId == legacyOwner).ToListAsync(cancellationToken);
            foreach (var legacyClub in legacyClubs) legacyClub.AssignOwner(owner.Id);
            await _db.SaveChangesAsync(cancellationToken);

            if (await _db.Clubs.AnyAsync(cancellationToken))
            {
                return;
            }

            foreach (var (city, suffix, basePrice, standardSeats, description) in Templates)
            {
                var club = new Club(
                    name: $"PClub {suffix}",
                    city: city,
                    address: $"ул. {suffix}, 1",
                    openingTime: new TimeOnly(10, 0),
                    closingTime: new TimeOnly(23, 0),
                    ownerId: owner.Id,
                    description: description);

                var standard = new Zone("Стандарт", basePrice);
                var vip = new Zone("VIP", basePrice * 2);

                club.AddZone(standard);
                club.AddZone(vip);

                for (var i = 1; i <= standardSeats; i++)
                {
                    standard.AddSeat(new Seat($"PC-{i:00}"));
                }

                for (var i = 1; i <= 3; i++)
                {
                    vip.AddSeat(new Seat($"VIP-{i:00}"));
                }

                var underRepair = new Seat($"PC-{standardSeats + 1:00}");
                underRepair.SendToMaintenance();
                standard.AddSeat(underRepair);

                club.ChangeStatus(ClubStatus.Published, hasBookableSeats: true);

                _db.Clubs.Add(club);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
