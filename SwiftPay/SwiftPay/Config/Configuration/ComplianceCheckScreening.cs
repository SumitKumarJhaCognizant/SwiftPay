using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Remittance.Entities;
using System;

namespace SwiftPay.Config.Configuration
{
    public class ComplianceScreeningConfiguration :
        IEntityTypeConfiguration<ComplianceCheck>,
        IEntityTypeConfiguration<ComplianceDecision>,
        IEntityTypeConfiguration<RoutingRule>,
        IEntityTypeConfiguration<PayoutInstruction>
    {
        // --- 1. ComplianceCheck Configuration ---
        public void Configure(EntityTypeBuilder<ComplianceCheck> builder)
        {
            builder.ToTable("ComplianceChecks");
            builder.HasKey(c => c.CheckId);
            builder.Property(c => c.CheckId).HasMaxLength(64).HasDefaultValueSql("NEWID()");
            builder.Property(c => c.CheckId).ValueGeneratedOnAdd();

            builder.Property(c => c.RemitId).IsRequired().HasMaxLength(64);

            // CHANGE 1: Convert CheckType Enum to String
            builder.Property(c => c.CheckType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(32);

            // CHANGE 2: Convert Result Enum to String & use Enum for Default Value
            builder.Property(c => c.Result)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20)
                .HasSentinel(ComplianceResult.Pending)
                .HasDefaultValue(ComplianceResult.Pending);

            // CHANGE 3: Convert Severity Enum to String & use Enum for Default Value
            builder.Property(c => c.Severity)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20)
                .HasSentinel(ComplianceSeverity.Low)
                .HasDefaultValue(ComplianceSeverity.Low);

            builder.Property(c => c.CheckedDate).HasDefaultValueSql("GETUTCDATE()");

            // Audit Fields
            builder.Property(c => c.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(c => c.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(c => c.IsDeleted).HasDefaultValue(false);

            builder.Property(c => c.RowVersion).IsRowVersion();
        }

        // --- 2. ComplianceDecision Configuration ---
        public void Configure(EntityTypeBuilder<ComplianceDecision> builder)
        {
            builder.ToTable("ComplianceDecisions");
            builder.HasKey(d => d.DecisionId);
            builder.Property(d => d.DecisionId).HasMaxLength(64).HasDefaultValueSql("NEWID()");
            builder.Property(d => d.DecisionId).ValueGeneratedOnAdd();

            builder.Property(d => d.RemitId).IsRequired().HasMaxLength(64);
            builder.Property(d => d.AnalystId).IsRequired().HasMaxLength(64);

            // --- UPDATED SECTION ---
            builder.Property(d => d.Decision)
                .HasConversion<string>() // Tells EF Core to save the Enum name as a string
                .IsRequired()
                .HasMaxLength(20);
            // -----------------------

            builder.Property(d => d.Notes).HasMaxLength(1000);
            builder.Property(d => d.DecisionDate).HasDefaultValueSql("GETUTCDATE()");

            // Audit Fields
            builder.Property(d => d.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(d => d.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(d => d.IsDeleted).HasDefaultValue(false);

            builder.Property(d => d.RowVersion).IsRowVersion();
        }

        // --- 3. RoutingRule Configuration ---
        public void Configure(EntityTypeBuilder<RoutingRule> builder)
        {
            builder.ToTable("RoutingRules");

            // Primary Key - matching your RuleID casing
            builder.HasKey(r => r.RuleId);
            builder.Property(r => r.RuleId).HasMaxLength(64).HasDefaultValueSql("NEWID()");
            builder.Property(r => r.RuleId).ValueGeneratedOnAdd();

            builder.Property(r => r.Corridor).IsRequired().HasMaxLength(16).IsUnicode(false);

            // --- UPDATED: PayoutMode with Enum Conversion ---
            builder.Property(r => r.PayoutMode)
                .HasConversion<string>() // Tells EF to store the Enum name (e.g., "BankTransfer")
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(r => r.PartnerCode).IsRequired().HasMaxLength(64);
            builder.Property(r => r.Priority).IsRequired().HasDefaultValue(1);

            // --- UPDATED: Status with Enum Conversion ---
            builder.Property(r => r.Status)
                .HasConversion<string>() // Tells EF to store the Enum name (e.g., "Active")
                .IsRequired()
                .HasMaxLength(20)
                .HasSentinel(RoutingRuleStatus.Inactive)
                .HasDefaultValue(RoutingRuleStatus.Active);

            // Audit Fields
            builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.IsDeleted).HasDefaultValue(false);

            builder.Property(r => r.RowVersion).IsRowVersion();
        }

        // --- 4. PayoutInstruction Configuration ---
        public void Configure(EntityTypeBuilder<PayoutInstruction> builder)
        {
            builder.ToTable("PayoutInstructions");

            // Primary Key
            builder.HasKey(p => p.InstructionId);
            builder.Property(p => p.InstructionId).HasMaxLength(64).HasDefaultValueSql("NEWID()");
            builder.Property(p => p.InstructionId).ValueGeneratedOnAdd();

            // Foreign Key & Partner Info
            builder.Property(p => p.RemitId).IsRequired().HasMaxLength(64);
            builder.Property(p => p.PartnerCode).IsRequired().HasMaxLength(64);

            // --- FEATURE: PAYLOAD JSON ---
            // This matches your model property: public string PayloadJson { get; set; }
            builder.Property(p => p.PayloadJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)"); // Allows large JSON data

            // Reference from Partner
            builder.Property(p => p.AckRef).HasMaxLength(128);

            // --- FEATURE: PARTNER STATUS ENUM ---
            builder.Property(p => p.PartnerStatus)
                .HasConversion<string>() // Saves as "Sent", "Ack", etc.
                .IsRequired()
                .HasMaxLength(32)
                .HasSentinel(PayOutInstructionStatus.Sent)
                .HasDefaultValue(PayOutInstructionStatus.Sent);

            builder.Property(p => p.SentDate).HasDefaultValueSql("GETUTCDATE()");

            // Audit Fields
            builder.Property(p => p.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(p => p.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(p => p.IsDeleted).HasDefaultValue(false);

            // Concurrency Token
            builder.Property(p => p.RowVersion).IsRowVersion();
        }
    }
}