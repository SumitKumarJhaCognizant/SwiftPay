using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftPay.Constants.Enums;
using SwiftPay.Domain.Notification.Entities;
using Model;

namespace SwiftPay.Config.Configuration
{
    public class NotificationAlertsConfiguration : IEntityTypeConfiguration<NotificationAlert>
    {
        public void Configure(EntityTypeBuilder<NotificationAlert> builder)
        {
            builder.HasKey(n => n.NotificationID);

            builder.Property(n => n.UserID).IsRequired();

            builder.Property(n => n.RemitID).IsRequired(false);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasColumnType("text");

            // Store enum as string so it maps to VARCHAR(50)
            builder.Property(n => n.Category)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            // Store enum as string so it maps to VARCHAR(50)
            builder.Property(n => n.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Unread");

            builder.Property(n => n.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign key to User
            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
