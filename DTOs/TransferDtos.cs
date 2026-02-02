using System.ComponentModel.DataAnnotations;

namespace BankLite.Api.Dtos
{
    // ✅ SECURITY: Input validation prevents negative amounts and invalid IBANs
    public record InitiateTransferRequest(
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid account ID")]
        int FromAccountId,
        
        [Required(ErrorMessage = "Recipient IBAN is required")]
        [StringLength(34, MinimumLength = 26, ErrorMessage = "IBAN must be between 26 and 34 characters")]
        [RegularExpression(@"^[A-Z]{2}[0-9]{2}[A-Z0-9]+$", ErrorMessage = "Invalid IBAN format")]
        string ToIban,
        
        [Required]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be between 0.01 and 1,000,000")]
        decimal Amount,
        
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        string? Description
    );

    public record InitiateTransferResponse(int TransferId, bool OtpRequired, string? OtpDevHint);

    public record ConfirmTransferRequest(
        [Required(ErrorMessage = "OTP code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only numbers")]
        string OtpCode
    );

    public record TransferResponse(int Id, int FromAccountId, string ToIban, decimal Amount, string? Description, string Status, DateTime CreatedAt);
}
