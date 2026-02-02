using System.Security.Cryptography;
using System.Text;

namespace BankLite.Api.Services
{
    public class PasswordService
    {
        // MVP: SHA256 (prod'da BCrypt/Argon2 önerilir)
        public string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public bool Verify(string password, string hash) => Hash(password) == hash;
    }
}
