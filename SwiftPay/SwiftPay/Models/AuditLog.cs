using System;
using Model;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class AuditLog
    {
        public int AuditID { get; set; }

        public int UserID { get; set; }

        public string Action { get; set; }

        public string Resource { get; set; }

        public DateTime Timestamp { get; set; }

        public User User { get; set; }
    }
}