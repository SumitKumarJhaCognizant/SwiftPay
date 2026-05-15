using System;
using System.ComponentModel.DataAnnotations;
using SwiftPay.Constants.Enums;

namespace SwiftPay.DTOs.PayoutDTO
{
	public class CreatePayoutInstructionDto
	{
		[Required]
		public string RemitId { get; set; } = string.Empty;

		[Required]
		public string PartnerCode { get; set; } = string.Empty;

		[Required]
		public string PayloadJson { get; set; } = string.Empty;

		public string? AckRef { get; set; }

		[Required]
		public PayOutInstructionStatus PartnerStatus { get; set; }
	}

	public class PayoutInstructionResponseDto : CreatePayoutInstructionDto
	{
		public string InstructionId { get; set; } = string.Empty;
		public DateTimeOffset SentDate { get; set; }
		public DateTimeOffset CreatedDate { get; set; }
	}

	public class UpdatePayoutStatusDto
	{
		[Required]
		public PayOutInstructionStatus Status { get; set; }

		public string? AckRef { get; set; }
	}
}