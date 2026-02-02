namespace BankLite.Api.Models
{
    public class Receipt
    {
        public int Id { get; set; }
        public int TransferId { get; set; }

        public string ReceiptNo { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // MVP: pdf yerine json/text
        public string ContentJson { get; set; } = default!;

        public Transfer? Transfer { get; set; }
    }
}
