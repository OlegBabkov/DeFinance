using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeFinance.Infrastructure.Persistence.Configurations;

public class OpeningBalanceOverrideConfiguration : IEntityTypeConfiguration<OpeningBalanceOverride>
{
    public void Configure(EntityTypeBuilder<OpeningBalanceOverride> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Year).IsRequired();
        builder.Property(e => e.Month).IsRequired();
        builder.Property(e => e.Amount).HasPrecision(18, 4);
        builder.Property(e => e.PlanAmount).HasPrecision(18, 4);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.Year, e.Month }).IsUnique();
    }
}
