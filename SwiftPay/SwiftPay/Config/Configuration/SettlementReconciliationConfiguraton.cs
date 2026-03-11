using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Models;

namespace SwiftPay.Config.Configuration
{
	public class SettlementBatchConfiguration : IEntityTypeConfiguration<SettlementBatch>
	{
		public void Configure(EntityTypeBuilder<SettlementBatch> builder)
		{
			builder.HasKey(s => s.BatchID);

			builder.Property(s => s.Corridor)
				   .IsRequired()
				   .HasMaxLength(10);

			builder.Property(s => s.PeriodStart)
				   .IsRequired();

			builder.Property(s => s.PeriodEnd)
				   .IsRequired();

			builder.Property(s => s.ItemCount)
				   .HasDefaultValue(0);

			builder.Property(s => s.TotalSendAmount)
				   .HasPrecision(15, 4)
				   .HasDefaultValue(0.0000m);

			builder.Property(s => s.TotalPayoutAmount)
				   .HasPrecision(15, 4)
				   .HasDefaultValue(0.0000m);

			builder.Property(s => s.CreatedDate)
				   .HasDefaultValueSql("GETDATE()");

			builder.Property(s => s.Status)
				   .HasMaxLength(50)
				   .HasDefaultValue("Open");
		}
	}
}
