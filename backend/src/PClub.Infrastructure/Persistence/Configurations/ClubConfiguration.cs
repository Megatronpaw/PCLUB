using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PClub.Domain.Entities;

namespace PClub.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Как класс Club превращвется в таблицу clubs.
    /// </summary>
    public sealed class ClubConfiguration : IEntityTypeConfiguration<Club>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Club> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
            builder.Property(c => c.City).HasMaxLength(100).IsRequired();
            builder.Property(c => c.Address).HasMaxLength(300);

            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(32);
            builder.HasMany(c => c.Zones)
                // То же, что у мест: навигация Zone.Club должна быть названа здесь,
                // иначе рядом с club_id появится теневая club_id1.
                .WithOne(z => z.Club)
                .HasForeignKey(z => z.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(c => new { c.City, c.Status });
            builder.Property(c => c.Description).HasMaxLength(2000);
        }
    }
}
