using System;

namespace SwiftPay.DTOs.FXQuoteDTO
{
    public class FXQuoteResponseDto
    {
        public string QuoteId { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal SendAmount { get; set; }
        public decimal ReceiverAmount { get; set; }
        public decimal MidRate { get; set; }
        public int MarginBps { get; set; }
        public decimal OfferedRate { get; set; }
        public decimal Fee { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
