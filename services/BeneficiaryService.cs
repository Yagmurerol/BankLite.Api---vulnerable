using BankLite.Api.Data;
using BankLite.Api.Dtos;
using BankLite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLite.Api.Services
{
    public class BeneficiaryService
    {
        private readonly BankLiteDbContext _db;

        public BeneficiaryService(BankLiteDbContext db)
        {
            _db = db;
        }

        // ✅ KAYITLI KİŞİLERİ LİSTELE
        public async Task<List<BeneficiaryDto>> ListAsync(int userId)
        {
            return await _db.Beneficiaries
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.Id)
                .Select(b => new BeneficiaryDto(b.Id, b.Name, b.Iban, b.BankName))
                .ToListAsync();
        }

        // ✅ KAYITLI KİŞİ EKLE
        public async Task<BeneficiaryDto> AddAsync(int userId, AddBeneficiaryRequest req)
        {
            var name = req.Name?.Trim() ?? throw new Exception("İsim boş olamaz");
            var iban = req.Iban?.Trim()?.ToUpperInvariant() ?? throw new Exception("IBAN boş olamaz");
            var bankName = req.BankName?.Trim() ?? "Unknown";

            // Benzersiz kontrol
            if (await _db.Beneficiaries.AnyAsync(b => b.UserId == userId && b.Iban == iban))
                throw new Exception("Bu IBAN zaten kayıtlı");

            var b = new Beneficiary
            {
                UserId = userId,
                Name = name,
                Iban = iban,
                BankName = bankName,
                CreatedAt = DateTime.UtcNow
            };

            _db.Beneficiaries.Add(b);
            await _db.SaveChangesAsync();

            return new BeneficiaryDto(b.Id, b.Name, b.Iban, b.BankName);
        }

        // ✅ KAYITLI KİŞİ SİL
        public async Task RemoveAsync(int userId, int beneficiaryId)
        {
            var b = await _db.Beneficiaries
                .FirstOrDefaultAsync(x => x.Id == beneficiaryId && x.UserId == userId);

            if (b == null)
                throw new Exception("Kayıtlı kişi bulunamadı");

            _db.Beneficiaries.Remove(b);
            await _db.SaveChangesAsync();
        }
    }
}
