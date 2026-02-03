using System.Security.Cryptography;
using System.Text;

namespace BankLite.Api.Services
{
    /// <summary>
    /// ✅ SECURITY: Masks sequential numeric IDs with hashed/obfuscated strings
    /// Prevents ID enumeration and IDOR attacks
    /// </summary>
    public class IdObfuscationService
    {
        private readonly string _secret;
        private const int MinHashLength = 16; // Produce at least 16 character hashes

        public IdObfuscationService(IConfiguration config)
        {
            // Use a consistent secret from appsettings
            _secret = config["Security:ObfuscationSecret"] 
                ?? throw new InvalidOperationException("Security:ObfuscationSecret not configured");
        }

        /// <summary>
        /// Obfuscate a numeric ID into a hashed string
        /// Example: 1 -> "a7km2nq9b3xv1f2e"
        /// </summary>
        public string Obfuscate(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be positive", nameof(id));

            var combined = $"{id}:{_secret}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            
            // Convert to hex string (32 chars for SHA256) - simple and always sufficient length
            var hex = Convert.ToHexString(hash).ToLowerInvariant();
            // Take first 16 characters for shorter obfuscated ID
            return hex.Substring(0, Math.Min(16, hex.Length));
        }

        /// <summary>
        /// Verify if an obfuscated ID matches the original numeric ID
        /// </summary>
        public bool Verify(int id, string obfuscated)
        {
            try
            {
                return Obfuscate(id) == obfuscated;
            }
            catch
            {
                return false;
            }
        }
    }
}
