using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PClub.Domain.Entities;

namespace PClub.Infrastructure.Persistence.Configurations;

/// <summary>Схема таблицы операций по кошельку.</summary>
public sealed class WalletTransactionConfiguration
    : IEntityTypeConfiguration<WalletTransaction>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(WalletTransaction.MaxDescriptionLength);
        builder.HasIndex(t => new { t.UserId, t.CreatedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Booking>()
            .WithMany()
            .HasForeignKey(t => t.BookingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
