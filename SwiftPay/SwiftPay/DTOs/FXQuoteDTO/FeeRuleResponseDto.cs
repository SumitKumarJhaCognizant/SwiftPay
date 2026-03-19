using System;
using SwiftPay.Constants.Enums;

namespace SwiftPay.DTOs.FXQuoteDTO
{
    public class FeeRuleResponseDto
    {
        public string FeeRuleID { get; set; }
        public string Corridor { get; set; }
        public PayoutMode PayoutMode { get; set; }
        public FeeType FeeType { get; set; }
        public decimal FeeValue { get; set; }
        public decimal MinFee { get; set; }
        public decimal MaxFee { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public RuleStatus Status { get; set; }
    }
}