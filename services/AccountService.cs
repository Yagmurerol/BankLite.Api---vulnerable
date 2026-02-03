using BankLite.Api.Data;
using BankLite.Api.Dtos;
using BankLite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLite.Api.Services
{
    public class AccountService
    {
        private readonly BankLiteDbContext _db;
        public AccountService(BankLiteDbContext db)
        {
            _db = db;
        }

        // ✅ HESAPLARI LİSTELE (with obfuscated IDs)
        public async Task<List<AccountResponse>> ListAsync(int userId)
        {
            return await _db.Accounts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Id)
                .Select(a => new AccountResponse(
                    a.Id,
                    a.Name,
                    a.Iban,
                    a.Currency,
                    a.Balance,
                    a.IsClosed
                ))
                .ToListAsync();
        }

        // ✅ HESAP AÇ (DB otomatik IBAN üretir)
        public async Task<AccountResponse> CreateAsync(int userId, CreateAccountRequest req)
        {
            var currency = (req.Currency ?? "TRY").Trim().ToUpperInvariant();

            if (currency != "TRY" && currency != "USD" && currency != "EUR")
                throw new Exception("Geçersiz para birimi");

            if (req.InitialDeposit < 0)
                throw new Exception("Başlangıç bakiyesi negatif olamaz");

            var name = string.IsNullOrWhiteSpace(req.Name)
                ? $"Vadesiz {currency}"
                : req.Name.Trim();

            // IBAN benzersiz üret (unique index var)
            for (int attempt = 0; attempt < 8; attempt++)
            {
                var iban = IbanGenerator.Generate().Trim().ToUpperInvariant();

                var acc = new Account
                {
                    UserId = userId,
                    Name = name,
                    Currency = currency,
                    Balance = req.InitialDeposit,
                    Iban = iban,
                    IsClosed = false,
                    CreatedAt = DateTime.UtcNow,
                    ClosedAt = null
                };

                _db.Accounts.Add(acc);

                try
                {
                    await _db.SaveChangesAsync();

                    return new AccountResponse(
                        acc.Id,
                        acc.Name,
                        acc.Iban,
                        acc.Currency,
                        acc.Balance,
                        acc.IsClosed
                    );
                }
                catch (DbUpdateException)
                {
                    // IBAN çakışması (çok düşük ihtimal) → yeniden dene
                }
            }

            throw new Exception("IBAN üretilemedi, tekrar deneyin");
        }

        // ✅ HESAP KAPAT
        public async Task CloseAsync(int userId, int accountId)
        {
            var acc = await _db.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (acc == null)
                throw new Exception("Hesap bulunamadı");

            if (acc.IsClosed)
                throw new Exception("Hesap zaten kapalı");

            // İstersen bunu kaldırabilirsin; ama bankacılık mantığında genelde 0 olmalı
            if (acc.Balance != 0)
                throw new Exception("Bakiye sıfır olmadan hesap kapatılamaz");

            acc.IsClosed = true;
            acc.ClosedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        // ✅ (TRANSFER / MEVDUAT İÇİN) Bu kullanıcıya ait hesabı getir
        public async Task<Account> GetOwnedAsync(int userId, int accountId)
        {
            var acc = await _db.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (acc == null)
                throw new Exception("Account not found");

            return acc;
        }

        // ✅ (TRANSFER İÇİN) IBAN ile hesap bul
        public async Task<Account?> FindByIbanAsync(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return null;

            var norm = iban.Trim().ToUpperInvariant();

            return await _db.Accounts
                .FirstOrDefaultAsync(a => a.Iban == norm);
        }
    }
}
