using System;
using Model;
using SwiftPay.Constants.Enums;

namespace SwiftPay.Domain.Notification.Entities
{
    public class NotificationAlert
    {
        public int NotificationID { get; set; }

        // Foreign key to User
        public int UserID { get; set; }

        // Foreign key to RemittanceRequest (optional)
        public int? RemitID { get; set; }

        // Message content
        public string Message { get; set; }

        // Notification category
        public NotificationCategory Category { get; set; }

        // Status of notification
        public NotificationStatus Status { get; set; }

        // Timestamp when created
        public DateTimeOffset CreatedDate { get; set; }

        // Navigation properties
        public User User { get; set; }
    }
}
