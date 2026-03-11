using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace SwiftPay.Config.Configuration
{
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
}