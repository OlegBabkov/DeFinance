using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeFinance.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.Color)
            .HasMaxLength(7);

        builder.Property(c => c.Icon)
            .HasMaxLength(50);

        builder.Property(c => c.PaymentObligation);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
