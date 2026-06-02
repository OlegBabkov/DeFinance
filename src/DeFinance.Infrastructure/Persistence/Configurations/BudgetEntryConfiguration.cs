using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeFinance.Infrastructure.Persistence.Configurations;

public class BudgetEntryConfiguration : IEntityTypeConfiguration<BudgetEntry>
{
    public void Configure(EntityTypeBuilder<BudgetEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Year).IsRequired();
        builder.Property(e => e.Month).IsRequired();
        builder.Property(e => e.PlannedAmount).HasPrecision(18, 4).IsRequired();

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.BudgetEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(e => e.Lines).HasField("_lines");

        builder.HasIndex(e => new { e.CategoryId, e.Year, e.Month }).IsUnique();
    }
}
