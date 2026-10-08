using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PClub.Domain.Entities;

namespace PClub.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Как класс Zone превращается в таблицу zones.
    /// </summary>
    public sealed class ZoneConfiguration : IEntityTypeConfiguration<Zone>
    {
        ///<inheritdoc />
        public void Configure(EntityTypeBuilder<Zone> builder)
        {
            builder.HasKey(z => z.Id);

            builder.Property(z => z.Name).HasMaxLength(100).IsRequired();
            builder.Property(z => z.Specs).HasMaxLength(500);
            builder.Property(z => z.PricePerHourCents).IsRequired();

            builder.HasMany(z => z.Seats)
                // WithOne(s => s.Zone), а не WithOne(): у места появилась навигация
                // вверх, и без её указания EF принял бы Seat.Zone за ВТОРУЮ связь
                // и завёл вдобавок теневую колонку zone_id1.
                .WithOne(s => s.Zone)
                .HasForeignKey(s => s.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);

            // Индекс по ВНЕШНЕМУ ключу, а не по первичному: по Id индекс уже
            // создан вместе с HasKey выше, и второй был бы чистым накладным
            // расходом — его бы обновляли при каждой вставке и никогда не читали.
            builder.HasIndex(z => z.ClubId);
        }
    }
}
