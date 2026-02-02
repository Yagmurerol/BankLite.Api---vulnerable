namespace BankLite.Api.Models
{
    public enum AlertType
    {
        Security,
        Transaction,
        Warning,
        Info
    }

    public class Alert
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public AlertType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }
}
