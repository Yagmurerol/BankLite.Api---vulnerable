namespace BankLite.Api.Dtos
{
    public class OpenAccountRequest
    {
        public string Name { get; set; } = default!;
        public string Currency { get; set; } = default!;
        public decimal InitialDeposit { get; set; }
    }
}
