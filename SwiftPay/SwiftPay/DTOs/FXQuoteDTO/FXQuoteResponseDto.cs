using System;

namespace SwiftPay.DTOs.FXQuoteDTO
{
    public class FXQuoteResponseDto
    {
        public string QuoteID { get; set; }
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal OfferedRate { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}