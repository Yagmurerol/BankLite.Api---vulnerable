using System.ComponentModel.DataAnnotations;

namespace BankLite.Api.Dtos
{
    public record BeneficiaryDto(
        int Id,
        string Name,
        string Iban,
        string BankName
    );

    // ✅ SECURITY: Input validation for beneficiary data
    public record AddBeneficiaryRequest(
        [Required(ErrorMessage = "Beneficiary name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        string Name,
        
        [Required(ErrorMessage = "IBAN is required")]
        [StringLength(34, MinimumLength = 26, ErrorMessage = "IBAN must be between 26 and 34 characters")]
        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]+$", ErrorMessage = "Invalid IBAN format")]
        string Iban,
        
        [Required(ErrorMessage = "Bank name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Bank name must be between 2 and 100 characters")]
        string BankName
    );
}
