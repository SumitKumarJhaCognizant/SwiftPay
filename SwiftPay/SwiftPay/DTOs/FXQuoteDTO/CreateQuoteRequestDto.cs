namespace SwiftPay.DTOs.FXQuoteDTO
{
    public class CreateQuoteRequestDto
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal SendAmount { get; set; }
    }
}
