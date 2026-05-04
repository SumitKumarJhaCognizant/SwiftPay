using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.FXModule.Api.Models;
using SwiftPay.Constants.Enums; // Added reference to your Enums namespace

namespace SwiftPay.Config.Configuration
{
    public class FXQuoteConfiguration : 
        IEntityTypeConfiguration<FXQuote>,
        IEntityTypeConfiguration<FeeRule>,
        IEntityTypeConfiguration<RateLock>
    {
        // 1. Configuration for FXQuote
        public void Configure(EntityTypeBuilder<FXQuote> builder)
        {
            builder.HasKey(f => f.QuoteID);
            builder.Property(f => f.QuoteID).HasDefaultValueSql("NEWID()");
            builder.Property(f => f.QuoteID).ValueGeneratedOnAdd();
            
            builder.Property(f => f.FromCurrency).IsRequired().HasMaxLength(3);
            builder.Property(f => f.ToCurrency).IsRequired().HasMaxLength(3);
            
            // CHANGED: Removed HasMaxLength and updated default value to the Enum type
            builder.Property(f => f.Status).IsRequired().HasDefaultValue(FXQuoteStatus.Active);
            
            builder.Property(f => f.QuoteTime).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(f => f.SendAmount).HasPrecision(18, 4);
            builder.Property(f => f.ReceiverAmount).HasPrecision(18, 4);
            builder.Property(f => f.MidRate).HasPrecision(18, 6);
            builder.Property(f => f.OfferedRate).HasPrecision(18, 6);
            builder.Property(f => f.Fee).HasPrecision(18, 4);

            builder.Property(f => f.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(f => f.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(f => f.IsDeleted).HasDefaultValue(false);
        }

        // 2. Configuration for FeeRule
        public void Configure(EntityTypeBuilder<FeeRule> builder)
        {
            builder.HasKey(f => f.FeeRuleID);
            builder.Property(f => f.FeeRuleID).HasDefaultValueSql("NEWID()");
            builder.Property(f => f.FeeRuleID).ValueGeneratedOnAdd();
            
            builder.Property(f => f.Corridor).IsRequired().HasMaxLength(7);
            
            // CHANGED: Removed string-specific constraints like HasMaxLength and string.Empty
            builder.Property(f => f.PayoutMode).IsRequired();
            builder.Property(f => f.FeeType).IsRequired();
            
            // CHANGED: Removed HasMaxLength and updated default value to the Enum type
            builder.Property(f => f.Status).IsRequired().HasDefaultValue(RuleStatus.Active);

            builder.Property(f => f.FeeValue).HasPrecision(18, 2);
            builder.Property(f => f.MinFee).HasPrecision(18, 2);
            builder.Property(f => f.MaxFee).HasPrecision(18, 2);

            builder.Property(f => f.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(f => f.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(f => f.IsDeleted).HasDefaultValue(false);
        }

        // 3. Configuration for RateLock
        public void Configure(EntityTypeBuilder<RateLock> builder)
        {
            builder.HasKey(r => r.LockID);
            builder.Property(r => r.LockID).HasDefaultValueSql("NEWID()");
            builder.Property(r => r.LockID).ValueGeneratedOnAdd();
            
            builder.Property(r => r.QuoteID).IsRequired().HasMaxLength(64);
            builder.Property(r => r.CustomerID).IsRequired().HasMaxLength(64);
            
            // CHANGED: Removed HasMaxLength and updated default value to the Enum type
            builder.Property(r => r.Status).IsRequired().HasDefaultValue(RateLockStatus.Locked);
            
            builder.Property(r => r.LockStart).HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.UpdateDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.IsDeleted).HasDefaultValue(false);
        }
    }
}