namespace BankLite.Api.Models
{
    public class Beneficiary
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Name { get; set; } = default!;
        public string Iban { get; set; } = default!;
        public string BankName { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
