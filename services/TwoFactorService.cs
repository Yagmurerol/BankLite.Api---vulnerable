namespace BankLite.Api.Services
{
    public class TwoFactorService
    {
        private readonly IConfiguration _cfg;
        private readonly Random _rng = new();

        public TwoFactorService(IConfiguration cfg) => _cfg = cfg;

        public bool IsEnabled() => bool.TryParse(_cfg["TwoFactor:Enabled"], out var b) && b;

        public bool ShouldRequireOtp(decimal amount)
        {
            var threshold = decimal.TryParse(_cfg["TwoFactor:AmountThreshold"], out var t) ? t : 5000m;
            return IsEnabled() && amount >= threshold;
        }

        public (string code, DateTime expiresAt) CreateOtp()
        {
            var len = int.TryParse(_cfg["TwoFactor:OtpLength"], out var l) ? l : 6;
            var ttl = int.TryParse(_cfg["TwoFactor:OtpTtlMinutes"], out var m) ? m : 5;

            var min = (int)Math.Pow(10, len - 1);
            var max = (int)Math.Pow(10, len) - 1;
            var code = _rng.Next(min, max).ToString();

            return (code, DateTime.UtcNow.AddMinutes(ttl));
        }

        public bool Verify(string input, string stored, DateTime? exp)
        {
            if (exp is null || exp < DateTime.UtcNow) return false;
            return (input ?? "").Trim() == (stored ?? "");
        }
    }
}
