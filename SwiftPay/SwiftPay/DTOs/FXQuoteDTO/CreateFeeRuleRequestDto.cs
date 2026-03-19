using System;
using SwiftPay.Constants.Enums;

namespace SwiftPay.DTOs.FXQuoteDTO
{
    public class CreateFeeRuleRequestDto
    {
        public string Corridor { get; set; } // e.g., "USD-INR"
        public PayoutMode PayoutMode { get; set; }
        public FeeType FeeType { get; set; }
        public decimal FeeValue { get; set; }
        public decimal MinFee { get; set; }
        public decimal MaxFee { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
    }
}