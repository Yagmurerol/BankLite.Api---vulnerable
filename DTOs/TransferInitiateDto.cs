namespace BankLite.Api.DTOs
{
    public class TransferInitiateDto
    {
        public int FromAccountId { get; set; }
        public string ToIban { get; set; } = default!;
        public decimal Amount { get; set; }
    }
}
