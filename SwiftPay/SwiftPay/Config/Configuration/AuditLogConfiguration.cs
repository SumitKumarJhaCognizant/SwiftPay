using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Domain.Remittance.Entities;

namespace SwiftPay.Config.Configuration
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.AuditID);

            builder.Property(a => a.UserID)
                .IsRequired();

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.Resource)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key to User
            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
