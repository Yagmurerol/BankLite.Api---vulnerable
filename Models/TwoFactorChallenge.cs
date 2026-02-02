namespace BankLite.Api.Models
{
    public class TwoFactorChallenge
    {
        public int Id { get; set; }

        // Keep only FK to avoid cascade path issues
        public int UserId { get; set; }

        public int TransferId { get; set; }
        public Transfer Transfer { get; set; } = default!;

        public string CodeHash { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
