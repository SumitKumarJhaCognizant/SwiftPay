using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using Model;

namespace SwiftPay.Config.Configuration
{
    public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
    {
        public void Configure(EntityTypeBuilder<CustomerProfile> builder)
        {
            builder.HasKey(c => c.CustomerID);

            builder.Property(c => c.UserID).IsRequired();
            builder.HasIndex(c => c.UserID).IsUnique();

            builder.Property(c => c.DOB).HasColumnType("date");
            builder.Property(c => c.AddressJSON).HasColumnType("text");
            builder.Property(c => c.Nationality).HasMaxLength(100);

                 // store enum as string so it maps to VARCHAR(50)
                 builder.Property(c => c.RiskRating)
                     .HasConversion<string>()
                     .IsRequired()
                     .HasMaxLength(50)
                     .HasDefaultValue("Low");

                 builder.Property(c => c.Status)
                     .HasConversion<string>()
                     .IsRequired()
                     .HasMaxLength(50)
                     .HasDefaultValue("Active");

            builder.HasOne(c => c.User)
                   .WithMany()
                   .HasForeignKey(c => c.UserID);
        }
    }

            // configuration for beneficiary entities
            public class BeneficiaryConfiguration : IEntityTypeConfiguration<Domain.Remittance.Entities.Beneficiary>
            {
             public void Configure(EntityTypeBuilder<Domain.Remittance.Entities.Beneficiary> builder)
             {
                 builder.HasKey(b => b.BeneficiaryID);

                 builder.Property(b => b.CustomerID).IsRequired();

                 builder.Property(b => b.Name)
                     .IsRequired()
                     .HasMaxLength(255);

                 builder.Property(b => b.Country)
                     .IsRequired()
                     .HasMaxLength(100);

                 builder.Property(b => b.PayoutMode)
                     .HasConversion<string>()
                     .IsRequired()
                     .HasMaxLength(50);

                 builder.Property(b => b.BankName)
                     .HasMaxLength(255);

                 builder.Property(b => b.BankCountry)
                     .HasMaxLength(100);

                 builder.Property(b => b.AccountOrWalletNo)
                     .IsRequired()
                     .HasMaxLength(100);

                 builder.HasIndex(b => b.AccountOrWalletNo)
                     .IsUnique();

                 builder.Property(b => b.IFSC_IBAN_SWIFT)
                     .HasMaxLength(50);

                 builder.Property(b => b.VerificationStatus)
                     .HasConversion<string>()
                     .IsRequired()
                     .HasMaxLength(50)
                     .HasDefaultValue("Pending");

                 builder.Property(b => b.AddedDate)
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");

                 builder.Property(b => b.Status)
                     .HasConversion<string>()
                     .IsRequired()
                     .HasMaxLength(50)
                     .HasDefaultValue("Active");

                 builder.HasOne(b => b.Customer)
                     .WithMany()
                     .HasForeignKey(b => b.CustomerID)
                     .OnDelete(DeleteBehavior.Restrict);
             }
            }
}