using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeFinance.Infrastructure.Persistence.Configurations;

public class ExchangeRateHistoryConfiguration : IEntityTypeConfiguration<ExchangeRateHistory>
{
    public void Configure(EntityTypeBuilder<ExchangeRateHistory> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Rate)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.HasIndex(e => new { e.CurrencyId, e.Date }).IsUnique();

        builder.HasOne(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
