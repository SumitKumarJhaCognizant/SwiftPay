using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Model;

namespace SwiftPay.Config.Configuration
{
    public class IdentityKycConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserId);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(255);

            // Map Enums to Strings
            builder.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            // Email with UNIQUE constraint
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            builder.HasIndex(u => u.Email).IsUnique();

            // Phone with UNIQUE constraint
            builder.Property(u => u.Phone)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(u => u.Phone).IsUnique();
        }
    }
}