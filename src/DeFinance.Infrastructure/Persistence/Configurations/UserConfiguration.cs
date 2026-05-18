using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeFinance.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Password)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
