using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeFinance.Infrastructure.Persistence.Configurations;

public class BudgetEntryLineConfiguration : IEntityTypeConfiguration<BudgetEntryLine>
{
    public void Configure(EntityTypeBuilder<BudgetEntryLine> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Amount).HasPrecision(18, 4).IsRequired();
        builder.Property(e => e.Order).IsRequired();
    }
}
