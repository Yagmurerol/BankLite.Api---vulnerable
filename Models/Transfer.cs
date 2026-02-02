namespace BankLite.Api.Models
{
    public enum TransferStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2
    }

    public class Transfer
    {
        public int Id { get; set; }
        public int FromAccountId { get; set; }

        public string ToIban { get; set; } = default!;
        public decimal Amount { get; set; }
        public string? Description { get; set; }

        public TransferStatus Status { get; set; } = TransferStatus.Pending;

        // OTP
        public bool OtpRequired { get; set; }
        public bool OtpVerified { get; set; }
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public Account? FromAccount { get; set; }
        public Receipt? Receipt { get; set; }
    }
}
