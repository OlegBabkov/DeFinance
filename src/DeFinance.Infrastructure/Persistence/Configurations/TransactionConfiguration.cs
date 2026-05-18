using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeFinance.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.DateTime)
            .IsRequired();

        builder.Property(t => t.Sum)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.ExchangeRate)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(t => t.AmountInCurrency)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.HasOne(t => t.InCurrency)
            .WithMany()
            .HasForeignKey(t => t.InCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Account)
            .WithMany()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Counterparty)
            .WithMany()
            .HasForeignKey(t => t.CounterpartyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.PaymentStatus)
            .WithMany()
            .HasForeignKey(t => t.PaymentStatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
