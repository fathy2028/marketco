using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Models
{
    public class Payment
    {
        [Key]
        public long PaymentId { get; set; }

        [Required]
        public long OrderId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(20)]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [StringLength(100)]
        public string? PaymentGateway { get; set; }

        public string? FailureReason { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public string? Currency { get; set; } = "USD";

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled,
        Refunded
    }

    public class PaymentRequest
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PaymentGateway { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class PaymentResponse
    {
        public long PaymentId { get; set; }
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentGateway { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Currency { get; set; }
        public string? Notes { get; set; }
    }

    public class RefundRequest
    {
        [Required]
        public long PaymentId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; } // If null, full refund

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
