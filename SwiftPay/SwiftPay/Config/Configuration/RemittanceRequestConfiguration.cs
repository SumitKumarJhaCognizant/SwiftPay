using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using SwiftPay.Models;

namespace SwiftPay.Config.Configuration
{
	public class RemittanceModuleConfiguration :
		IEntityTypeConfiguration<RemittanceRequest>,
		IEntityTypeConfiguration<RemitValidation>,
		IEntityTypeConfiguration<Document>
	{
		// --- 1. RemittanceRequest Configuration ---
		public void Configure(EntityTypeBuilder<RemittanceRequest> builder)
		{
			builder.ToTable("RemittanceRequests");
            builder.HasKey(r => r.RemitId);
            // RemitId is stored as string GUID (varchar) across related tables
            builder.Property(r => r.RemitId)
                .IsRequired()
                .HasMaxLength(64)
                .ValueGeneratedOnAdd();

			builder.Property(r => r.FromCurrency).IsRequired().HasMaxLength(3).IsFixedLength().IsUnicode(false);
			builder.Property(r => r.ToCurrency).IsRequired().HasMaxLength(3).IsFixedLength().IsUnicode(false);

			builder.Property(r => r.SendAmount).HasPrecision(18, 2);
			builder.Property(r => r.ReceiverAmount).HasPrecision(18, 2);
			builder.Property(r => r.FeeApplied).HasPrecision(18, 2);
			builder.Property(r => r.RateApplied).HasPrecision(18, 4);

			builder.Property(r => r.Status)
				.HasConversion<string>()
				.HasMaxLength(30)
				.IsUnicode(false)
				.HasDefaultValue(RemittanceRequestStatus.Draft);

			builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
			builder.Property(r => r.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
			builder.Property(r => r.IsDeleted).HasDefaultValue(false);

			// Relationships
			builder.HasMany<RemitValidation>()
				.WithOne(v => v.RemittanceRequest)
				.HasForeignKey(v => v.RemitId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany<Document>()
				.WithOne(d => d.RemittanceRequest)
				.HasForeignKey(d => d.RemitId)
				.OnDelete(DeleteBehavior.Cascade);
			//FOREIGN KEY
			// --- Add these inside public void Configure(EntityTypeBuilder<RemittanceRequest> builder) ---

			// 1. Foreign Key to Customer
			builder.HasOne(r => r.Customer)
				.WithMany() // Or .WithMany(c => c.Remittances) if the navigation exists in Customer class
				.HasForeignKey(r => r.CustomerId)
				.OnDelete(DeleteBehavior.Restrict); // Best practice for financial records

			// 2. Foreign Key to Beneficiary
			builder.HasOne(r => r.Beneficiary)
				.WithMany()
				.HasForeignKey(r => r.BeneficiaryId)
				.OnDelete(DeleteBehavior.Restrict);

			// 3. Foreign Key to Quote
			// Explicitly link QuoteId to FXQuote table
            builder.HasOne(r => r.FXQuote)
                   .WithMany()
                   .HasForeignKey(r => r.QuoteId)
                   .OnDelete(DeleteBehavior.Restrict);
		}

		// --- 2. RemitValidation Configuration ---
		public void Configure(EntityTypeBuilder<RemitValidation> builder)
		{
			builder.ToTable("RemitValidations");
			builder.HasKey(v => v.ValidationId);
				builder.Property(v => v.ValidationId).HasDefaultValueSql("NEWID()").ValueGeneratedOnAdd();

				builder.Property(v => v.RemitId)
					.IsRequired()
					.HasMaxLength(64);

			builder.Property(v => v.RuleName).IsRequired().HasMaxLength(100);
			builder.Property(v => v.Message).HasMaxLength(500);

			builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
			builder.Property(r => r.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
			builder.Property(r => r.IsDeleted).HasDefaultValue(false);

			// Explicitly configure the relationship from the dependent side as well
			// to avoid EF creating a shadow FK column (e.g. RemittanceRequestRemitId).
			builder.HasOne(v => v.RemittanceRequest)
				.WithMany(r => r.Validations)
				.HasForeignKey(v => v.RemitId)
				.HasPrincipalKey(r => r.RemitId)
				.OnDelete(DeleteBehavior.Cascade);

		}

		// --- 3. Document Configuration ---
		public void Configure(EntityTypeBuilder<Document> builder)
		{
			builder.ToTable("RemittanceDocuments");
			builder.HasKey(d => d.DocumentId);
			builder.Property(d => d.DocumentId).ValueGeneratedOnAdd();

				builder.Property(d => d.DocType).IsRequired().HasMaxLength(50);

				builder.Property(d => d.RemitId)
					.IsRequired()
					.HasMaxLength(64);
			builder.Property(d => d.FileURI).IsRequired().HasMaxLength(2048);

            builder.Property(d => d.VerificationStatus)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue(VerificationStatus.Pending);

			builder.Property(d => d.UploadedDate).HasDefaultValueSql("GETUTCDATE()");

			builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
			builder.Property(r => r.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
			builder.Property(r => r.IsDeleted).HasDefaultValue(false);

			// Configure dependent -> principal mapping explicitly
			builder.HasOne(d => d.RemittanceRequest)
				.WithMany(r => r.Documents)
				.HasForeignKey(d => d.RemitId)
				.HasPrincipalKey(r => r.RemitId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}