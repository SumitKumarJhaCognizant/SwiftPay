using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using Model;

namespace SwiftPay.Config.Configuration
{
    // User Configuration
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserId);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(255);

            // Email with UNIQUE constraint
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            builder.HasIndex(u => u.Email).IsUnique();

            // Phone with UNIQUE constraint
            builder.Property(u => u.Phone)
                .HasMaxLength(50);
            builder.HasIndex(u => u.Phone).IsUnique();

            // Password
            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            // IsDeleted with default value false
            builder.Property(u => u.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // CreatedAt with default current timestamp
            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // UpdatedAt with default current timestamp
            builder.Property(u => u.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Navigation
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // Role Configuration
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.RoleId);

            builder.Property(r => r.RoleType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(r => r.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Navigation
            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // UserRole Configuration
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(ur => ur.UserRoleId);

            builder.Property(ur => ur.UserId).IsRequired();
            builder.Property(ur => ur.RoleId).IsRequired();

            builder.Property(ur => ur.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(ur => ur.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(ur => ur.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Foreign keys
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // KYC Record Configuration
    public class KYCRecordConfiguration : IEntityTypeConfiguration<KYCRecord>
    {
        public void Configure(EntityTypeBuilder<KYCRecord> builder)
        {
            builder.HasKey(k => k.KYCID);

            builder.Property(k => k.UserID)
                .IsRequired();

            // Map KYCLevel enum to VARCHAR
            builder.Property(k => k.KYCLevel)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            // Map VerificationStatus enum to VARCHAR with default "Pending"
            builder.Property(k => k.VerificationStatus)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            builder.Property(k => k.VerifiedDate)
                .IsRequired(false);

            builder.Property(k => k.Notes)
                .HasColumnType("text")
                .IsRequired(false);

            // CreatedAt with default current timestamp
            builder.Property(k => k.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // UpdatedAt with default current timestamp
            builder.Property(k => k.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // IsDeleted with default value false
            builder.Property(k => k.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Foreign key to User
            builder.HasOne(k => k.User)
                .WithMany()
                .HasForeignKey(k => k.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}