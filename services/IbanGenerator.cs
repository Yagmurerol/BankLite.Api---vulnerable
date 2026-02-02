using System.Security.Cryptography;

namespace BankLite.Api.Services
{
    public static class IbanGenerator
    {
        // TR + 24 digit
        public static string Generate()
        {
            Span<byte> bytes = stackalloc byte[12];
            RandomNumberGenerator.Fill(bytes);

            var digits = new char[24];
            for (int i = 0; i < 24; i++)
                digits[i] = (char)('0' + (bytes[i % bytes.Length] % 10));

            return "TR" + new string(digits);
        }
    }
}
