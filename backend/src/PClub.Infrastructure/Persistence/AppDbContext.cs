using Microsoft.EntityFrameworkCore;
using PClub.Domain.Entities;

namespace PClub.Infrastructure.Persistence
{
    /// <summary>
    /// Контекст базы данных: единая точка доступа ко всем таблицам.
    /// </summary>
    /// <remarks>
    /// Настройки приходят снаружи, через конструктор, — поэтому контекст
    /// ничего не знает ни про строку подключения, ни про то, что под ним
    /// PostgreSQL. Это решает Program.cs при регистрации.
    /// </remarks>
    public sealed class AppDbContext : DbContext
    {
        /// <summary>
        /// Создаёт контекст с переданными настройками.
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        /// <summary>Пользователи</summary>
        public DbSet<User> Users => Set<User>();
        /// <summary>Клубы.</summary>
        public DbSet<Club> Clubs => Set<Club>();
        /// <summary>Зоны.</summary>
        public DbSet<Zone> Zones => Set<Zone>();
        /// <summary>Места.</summary>
        public DbSet<Seat> Seats => Set<Seat>();
        /// <summary>Брони.</summary>
        public DbSet<Booking> Bookings => Set<Booking>();

        ///<inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
