namespace BankLite.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Account> Accounts { get; set; } = new();
        public List<Beneficiary> Beneficiaries { get; set; } = new();
    }
}
