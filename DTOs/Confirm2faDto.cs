namespace BankLite.Api.DTOs
{
    public class Confirm2faDto
    {
        public int ChallengeId { get; set; }
        public string Code { get; set; } = default!;
    }
}
