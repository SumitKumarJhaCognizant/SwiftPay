namespace SwiftPay.DTOs.FXQuoteDTO
{
    public class CreateRateLockRequestDto
    {
        public string QuoteID { get; set; }
        public string ?CustomerID { get; set; }
    }
}