using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PClub.Domain.Entities;

namespace PClub.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Как класс Booking превращвется в таблицу bookings.
    /// </summary>
    public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(32);
            builder.Property(b => b.TotalPriceCents).IsRequired();

            builder.HasOne<Seat>()
                .WithMany()
                .HasForeignKey(b => b.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => new { b.SeatId, b.StartTime });

            builder.Ignore(b => b.Duration);
        }
    }
}
