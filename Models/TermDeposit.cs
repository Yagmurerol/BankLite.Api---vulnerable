namespace BankLite.Api.Models
{
    public enum TermDepositStatus
    {
        Open = 0,
        Closed = 1
    }

    public class TermDeposit
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public int FromAccountId { get; set; }      // paranın çekildiği vadesiz
        public int PayoutAccountId { get; set; }    // bozulunca geri yatacağı vadesiz

        public decimal Principal { get; set; }      // ana para
        public decimal Rate { get; set; }           // ör: 0.35 => %35
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime MaturityDate { get; set; }

        public TermDepositStatus Status { get; set; } = TermDepositStatus.Open;
        public DateTime? ClosedAt { get; set; }

        public decimal? PayoutAmount { get; set; }

        public User? User { get; set; }
    }
}
