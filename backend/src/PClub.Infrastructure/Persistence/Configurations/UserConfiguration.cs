using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PClub.Domain.Entities;

namespace PClub.Infrastructure.Persistence.Configurations
{
    /// <summary>Схема таблицы пользователей.</summary>
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.DisplayName)
                .IsRequired()
                .HasMaxLength(User.MaxDisplayNameLength);

            // Уникальность почты — на уровне базы. Проверка в коде её не заменяет:
            // между проверкой и вставкой тот же зазор, что и у броней.
            builder.HasIndex(u => u.Email).IsUnique();

            builder.HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                // Внешний ключ — скалярное свойство UserId, а не b.User.Id:
                // EF нужна колонка, а не путь через навигацию.
                .HasForeignKey(b => b.UserId)
                // Restrict: пользователя с бронями не удалить. Брони — финансовая
                // история, её нельзя терять вместе с учётной записью.
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
