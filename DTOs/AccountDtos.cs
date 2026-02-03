using System.ComponentModel.DataAnnotations;

namespace BankLite.Api.Dtos
{
    // ✅ SECURITY: Input validation for account creation
    public record CreateAccountRequest(
        [Required(ErrorMessage = "Account name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Account name must be between 2 and 100 characters")]
        string Name,
        
        [Required(ErrorMessage = "Currency is required")]
        [RegularExpression(@"^(TRY|USD|EUR|GBP)$", ErrorMessage = "Currency must be TRY, USD, EUR, or GBP")]
        string Currency,
        
        [Required]
        [Range(0, 1000000, ErrorMessage = "Initial deposit must be between 0 and 1,000,000")]
        decimal InitialDeposit
    );

    public record AccountResponse(
        string Id,  // ✅ SECURITY: Obfuscated ID to prevent enumeration attacks
        string Name,
        string Iban,
        string Currency,
        decimal Balance,
        bool IsClosed
    );
}
