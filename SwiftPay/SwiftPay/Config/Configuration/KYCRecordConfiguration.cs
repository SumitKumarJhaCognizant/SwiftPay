using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Config.Configuration
{
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

            // Foreign key to User
            builder.HasOne(k => k.User)
                .WithMany()
                .HasForeignKey(k => k.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
