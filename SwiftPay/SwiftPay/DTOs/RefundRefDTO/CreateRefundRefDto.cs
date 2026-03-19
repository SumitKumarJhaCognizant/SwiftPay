using System;
using System.ComponentModel.DataAnnotations;
using SwiftPay.Constants.Enums;

namespace SwiftPay.DTOs.RefundRefDTO
{
    public class CreateRefundRefDto
    {
        [Required]
        public int RemitId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public DateTime? RefundDate { get; set; }

        [Required]
        public RefundMethod Method { get; set; }
    }
}
