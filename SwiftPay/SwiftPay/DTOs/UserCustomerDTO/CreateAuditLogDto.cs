using System;

namespace SwiftPay.DTOs.UserCustomerDTO
{
    public class CreateAuditLogDto
    {
        public int UserID { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
