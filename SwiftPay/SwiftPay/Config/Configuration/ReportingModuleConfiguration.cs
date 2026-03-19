using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Config.Configuration
{
    public class ReportingModuleConfiguration : IEntityTypeConfiguration<RemitReport>
    {
        public void Configure(EntityTypeBuilder<RemitReport> builder)
        {
            builder.ToTable("RemitReports");
            builder.HasKey(r => r.ReportID);
            builder.Property(r => r.ReportID).ValueGeneratedOnAdd();
            
            builder.Property(r => r.Scope).IsRequired().HasMaxLength(50);
            builder.Property(r => r.Metrics).HasColumnType("text");
            
            builder.Property(r => r.GeneratedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.UpdatedDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.IsDeleted).HasDefaultValue(false);
        }
    }
}