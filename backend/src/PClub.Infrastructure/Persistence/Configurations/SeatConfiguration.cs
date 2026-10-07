using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PClub.Domain.Entities;

namespace PClub.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Как класс Seat превращвется в таблицу seats.
    /// </summary>
    public sealed class SeatConfiguration : IEntityTypeConfiguration<Seat>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<Seat> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Label).HasMaxLength(50).IsRequired();
            builder.Property(s => s.Specs).HasMaxLength(500);
            builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(32);
            builder.HasIndex(s => new { s.ZoneId, s.Label }).IsUnique();
        }
    }
}
