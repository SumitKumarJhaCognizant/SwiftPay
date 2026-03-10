using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftPay.Domain.Remittance.Entities
{
    public class Amendment
    {
        // Primary Key
        [Key]
        [MaxLength(64)]
        public string AmendmentId { get; set; } = default!;

        // Foreign references (by ID only for Phase-1)
        // FK → RemittanceRequest(RemitId)
        [Required, MaxLength(64)]
        public string RemitId { get; set; } = default!;

        // Which field changed
        [Required, MaxLength(100)]
        public string FieldChanged { get; set; } = default!;

        // Old/New values (TEXT in DB)
        [Column(TypeName = "text")]
        public string? OldValue { get; set; }

        [Column(TypeName = "text")]
        public string? NewValue { get; set; }

        // Who requested (FK → Users(UserId)) — IDs-only in Phase-1
        [Required, MaxLength(64)]
        public string RequestedByUserId { get; set; } = default!;

        // When requested
        [Required]
        public DateTimeOffset RequestedDate { get; set; } = DateTimeOffset.UtcNow;

        // Status: Pending | Approved | Rejected (string for Phase-1, to mirror your pattern)
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        // Audit (mirrors RemittanceRequest.cs)
        [Required]
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedDate { get; set; }

        [MaxLength(64)]
        public string? CreatedByUserId { get; set; }

        [MaxLength(64)]
        public string? UpdatedByUserId { get; set; }

        // Optimistic concurrency
        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}