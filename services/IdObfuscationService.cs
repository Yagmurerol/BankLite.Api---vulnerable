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
        /// Obfuscate a numeric ID into a hashed string (base62)
        /// Example: 1 -> "A7kM2nQ9B3xV"
        /// </summary>
        public string Obfuscate(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be positive", nameof(id));

            var combined = $"{id}:{_secret}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            
            // Convert to base62 for shorter, URL-safe representation
            return Base62Encode(hash).Substring(0, MinHashLength);
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

        private static string Base62Encode(byte[] bytes)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var result = new StringBuilder();
            
            var num = new System.Numerics.BigInteger(bytes);
            var baseNum = new System.Numerics.BigInteger(62);

            while (num > 0)
            {
                var remainder = (int)(num % baseNum);
                result.Insert(0, chars[remainder]);
                num /= baseNum;
            }

            return result.Length == 0 ? "0" : result.ToString();
        }
    }
}
