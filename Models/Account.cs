namespace BankLite.Api.Models
{
    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; } = "Vadesiz TL";
        public string Iban { get; set; } = default!;
        public string Currency { get; set; } = "TRY";

        public decimal Balance { get; set; } = 0m;

        public bool IsClosed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public User? User { get; set; }
    }
}
