using System.ComponentModel.DataAnnotations;

namespace BankLite.Api.Dtos
{
    // ✅ SECURITY: Input validation added to prevent injection and ensure data quality
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
        public string Username { get; set; } = default!;
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = default!;
        
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string FullName { get; set; } = default!;

        public string? Email { get; set; }
    }

    public record LoginRequest(
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
        string Username,
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "Password too long")]
        string Password
    );

    // ✅ FIXED: OldPassword field added for secure password change
    public record ChangePasswordRequest(
        [Required(ErrorMessage = "Current password is required")]
        string OldPassword,
        
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        string NewPassword
    );

    // ✅ FIXED: Role field removed from UpdateUserRequest to prevent Mass Assignment
    public record UpdateUserRequest(
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
        string? Username,
        
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        string? FullName
    );

    public record AuthResponse(string Token, string Username, string FullName);
}
