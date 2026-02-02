using System.Text;

namespace BankLite.Api.Services
{
    public class IbanService
    {
        private static readonly Random _rng = new();

        // Basit TR IBAN üretimi (MVP). Prod'da resmi algoritma/validasyon gerekli.
        public string GenerateTrIban()
        {
            // TR + 2 checksum + 5 banka + 1 rezerv + 16 hesap numarası = 26 char
            // MVP: checksum'ı random bırakıyoruz, DB unique ile çakışmayı engelliyoruz.
            var checksum = _rng.Next(10, 99).ToString();
            var bank = _rng.Next(10000, 99999).ToString();
            var reserve = _rng.Next(0, 9).ToString();
            var acc = _rng.NextInt64(1000000000000000, 9999999999999999).ToString();

            return $"TR{checksum}{bank}{reserve}{acc}";
        }

        public string Normalize(string iban) => iban.Replace(" ", "").Trim().ToUpperInvariant();

        public bool LooksValidTr(string iban)
        {
            iban = Normalize(iban);
            return iban.StartsWith("TR") && iban.Length == 26;
        }
    }
}
