using System;
using SwiftPay.Constants.Enums;

namespace SwiftPay.DTOs.FXQuoteDTO
{
    public class RateLockResponseDto
    {
        public string LockID { get; set; }
        public string QuoteID { get; set; }
        public string CustomerID { get; set; }
        public RateLockStatus Status { get; set; }
        public DateTime LockStart { get; set; }
    }
}