using System;
using System.Collections.Generic;
using System.Text;
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
            if (await _db.Clubs.AnyAsync(cancellationToken))
            {
                return;
            }

            var club = new Club(
                "Neon Arena", "Москва", "ул. Ленина, 52",
                new TimeOnly(12, 0), new TimeOnly(0, 0));

            var standard = new Zone("Standard", 15_000, "i5-13400F · RTX 4060 · 27\" 165Гц");
            standard.AddSeat(new Seat("PC-01"));
            standard.AddSeat(new Seat("PC-02"));

            var underRepair = new Seat("PC-03");
            underRepair.SendToMaintenance();
            standard.AddSeat(underRepair);

            var vip = new Zone("VIP", 30_000, "i9-14900K · RTX 5080 · 27\" 370 Гц");
            vip.AddSeat(new Seat("VIP-01"));

            club.AddZone(standard);
            club.AddZone(vip);
            club.ChangeStatus(ClubStatus.Published, hasBookableSeats: true);

            _db.Clubs.Add(club);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
