namespace BankLite.Api.Dtos
{
    // ✅ SECURITY: ID is obfuscated to prevent enumeration attacks
    public record AccountResponse(
        string Id,  // Obfuscated ID (was: int Id)
        string Name,
        string Iban,
        string Currency,
        decimal Balance,
        bool IsClosed
    );
}
