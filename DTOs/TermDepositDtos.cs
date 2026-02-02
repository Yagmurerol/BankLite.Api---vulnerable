using System.ComponentModel.DataAnnotations;

namespace BankLite.Api.Dtos
{
    // ✅ SECURITY: Input validation for term deposit
    public record OpenDepositRequest(
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid source account ID")]
        int FromAccountId,
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid payout account ID")]
        int PayoutAccountId,
        
        [Required]
        [Range(100, 10000000, ErrorMessage = "Principal must be between 100 and 10,000,000")]
        decimal Principal,
        
        [Required]
        [Range(30, 3650, ErrorMessage = "Days must be between 30 and 3650 (10 years)")]
        int Days,
        
        [Required]
        [Range(0.01, 100, ErrorMessage = "Rate must be between 0.01 and 100")]
        decimal Rate
    );

    public record DepositResponse(int Id, int FromAccountId, int PayoutAccountId, decimal Principal, decimal Rate, DateTime StartDate, DateTime MaturityDate, string Status);
}
