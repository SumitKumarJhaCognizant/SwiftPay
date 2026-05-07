using System.Text.Json.Serialization; // Added for JsonIgnore

namespace SwiftPay.DTOs.FXQuoteDTO
{
    public class CreateQuoteRequestDto
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal SendAmount { get; set; }
        
        // --- ADDED: Hidden property to securely carry the ID to the service ---
        [JsonIgnore]
        public string? CustomerID { get; set; }
    }
}
