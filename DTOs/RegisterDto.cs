namespace BankLite.Api.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = default!; // customerId buraya gelecek
        public string Password { get; set; } = default!;
        public string? FullName { get; set; } // artýk opsiyonel
        public string? Email { get; set; } // mail topluyorsan ekle
    }
}
