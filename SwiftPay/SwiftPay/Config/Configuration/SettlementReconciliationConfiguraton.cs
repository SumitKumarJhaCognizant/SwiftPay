using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Models;

namespace SwiftPay.Config.Configuration
{
    // ReconciliationRecord configuration
    public class ReconciliationRecordConfiguration : IEntityTypeConfiguration<ReconciliationRecord>
    {
        public void Configure(EntityTypeBuilder<ReconciliationRecord> builder)
        {
            builder.HasKey(r => r.ReconID);
            builder.Property(r => r.ReferenceType).IsRequired().HasMaxLength(50);
            builder.Property(r => r.ReferenceID).IsRequired().HasMaxLength(100);
            builder.Property(r => r.ExpectedAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(r => r.ActualAmount).IsRequired().HasPrecision(18, 4);
            builder.Property(r => r.Result).IsRequired().HasMaxLength(50);
            builder.Property(r => r.ReconDate).IsRequired().HasDefaultValueSql("GETDATE()");
        }
    }

    // SettlementBatch configuration
    public class SettlementBatchConfiguration : IEntityTypeConfiguration<SettlementBatch>
    {
        public void Configure(EntityTypeBuilder<SettlementBatch> builder)
        {
            builder.HasKey(s => s.BatchID);

            builder.Property(s => s.Corridor)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(s => s.PeriodStart).IsRequired();
            builder.Property(s => s.PeriodEnd).IsRequired();

            builder.Property(s => s.ItemCount).HasDefaultValue(0);

            builder.Property(s => s.TotalSendAmount)
                   .HasPrecision(15, 4)
                   .HasDefaultValue(0.0000m);

            builder.Property(s => s.TotalPayoutAmount)
                   .HasPrecision(15, 4)
                   .HasDefaultValue(0.0000m);

            builder.Property(s => s.CreatedDate)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.Status)
                   .HasConversion<string>() // store enum as string
                   .HasMaxLength(50)
                   .HasDefaultValue("Open");
        }
    }
}
