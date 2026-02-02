using BankLite.Api.Data;
using BankLite.Api.Dtos;
using BankLite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLite.Api.Services
{
    public class TransferService
    {
        private readonly BankLiteDbContext _db;
        private readonly AccountService _accounts;
        private readonly IbanService _iban;
        private readonly TwoFactorService _tfa;
        private readonly ReceiptService _receipts;
        private readonly AlertService _alerts;

        public TransferService(
            BankLiteDbContext db,
            AccountService accounts,
            IbanService iban,
            TwoFactorService tfa,
            ReceiptService receipts,
            AlertService alerts)
        {
            _db = db;
            _accounts = accounts;
            _iban = iban;
            _tfa = tfa;
            _receipts = receipts;
            _alerts = alerts;
        }

        public async Task<InitiateTransferResponse> InitiateAsync(int userId, InitiateTransferRequest req)
        {
            if (req.Amount <= 0) throw new InvalidOperationException("Amount must be > 0");

            var toIban = _iban.Normalize(req.ToIban);
            if (!_iban.LooksValidTr(toIban)) throw new InvalidOperationException("Invalid IBAN");

            // From account ownership
            var from = await _accounts.GetOwnedAsync(userId, req.FromAccountId);
            if (from.IsClosed) throw new InvalidOperationException("Account closed");
            if (from.Balance < req.Amount) throw new InvalidOperationException("Yetersiz bakiye");

            // To account exists?
            var toAcc = await _accounts.FindByIbanAsync(toIban);
            if (toAcc is null) throw new InvalidOperationException("Hedef IBAN bulunamadı");
            if (toAcc.IsClosed) throw new InvalidOperationException("Hedef hesap kapalı");
            if (toAcc.Currency != from.Currency) throw new InvalidOperationException("Para birimi farklı (MVP’de engelli)");

            var t = new Transfer
            {
                FromAccountId = from.Id,
                ToIban = toIban,
                Amount = req.Amount,
                Description = req.Description,
                Status = TransferStatus.Pending
            };

            // OTP?
            if (_tfa.ShouldRequireOtp(req.Amount))
            {
                var (code, exp) = _tfa.CreateOtp();
                t.OtpRequired = true;
                t.OtpCode = code;
                t.OtpExpiresAt = exp;
                t.OtpVerified = false;
            }

            _db.Transfers.Add(t);
            await _db.SaveChangesAsync();

            // DEV HINT: otp'yi döndürüyoruz (prod'da SMS/email)
            var hint = t.OtpRequired ? t.OtpCode : null;

            // OTP gerekmiyorsa otomatik tamamla
            if (!t.OtpRequired)
            {
                await CompleteAsync(userId, t.Id, otp: null);
            }

            return new InitiateTransferResponse(t.Id, t.OtpRequired, hint);
        }

        public async Task CompleteAsync(int userId, int transferId, string? otp)
        {
            // Transfer'i getir
            var t = await _db.Transfers.FirstOrDefaultAsync(x => x.Id == transferId);
            if (t is null) throw new InvalidOperationException("Transfer not found");

            // Ownership: FromAccount user'a ait mi?
            var from = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == t.FromAccountId);
            if (from is null) throw new InvalidOperationException("From account missing");
            if (from.UserId != userId) throw new InvalidOperationException("Not allowed");

            if (t.Status != TransferStatus.Pending) return;

            // OTP gerekiyorsa doğrula
            if (t.OtpRequired)
            {
                if (!t.OtpVerified)
                {
                    if (!_tfa.Verify(otp ?? "", t.OtpCode ?? "", t.OtpExpiresAt))
                        throw new InvalidOperationException("OTP hatalı veya süresi doldu");

                    t.OtpVerified = true;
                }
            }

            // To account
            var toAcc = await _db.Accounts.FirstOrDefaultAsync(a => a.Iban == t.ToIban);
            if (toAcc is null) throw new InvalidOperationException("To account not found");
            if (toAcc.IsClosed) throw new InvalidOperationException("To account closed");

            // Transaction
            using var trx = await _db.Database.BeginTransactionAsync();
            try
            {
                // refresh balances
                await _db.Entry(from).ReloadAsync();
                await _db.Entry(toAcc).ReloadAsync();

                if (from.IsClosed) throw new InvalidOperationException("From account closed");
                if (from.Balance < t.Amount) throw new InvalidOperationException("Yetersiz bakiye");

                from.Balance -= t.Amount;
                toAcc.Balance += t.Amount;

                t.Status = TransferStatus.Completed;
                t.CompletedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                // Receipt
                await _receipts.CreateForTransferAsync(t);

                // Alert oluştur (transfer + bakiye uyarıları)
                await _alerts.CreateTransferAlertAsync(userId, t.Amount, t.ToIban, from.Balance);

                await trx.CommitAsync();
            }
            catch
            {
                await trx.RollbackAsync();
                t.Status = TransferStatus.Failed;
                await _db.SaveChangesAsync();
                throw;
            }
        }

        public async Task<List<TransferResponse>> ListAsync(int userId)
        {
            var list = await _db.Transfers
                .Include(t => t.FromAccount)
                .Where(t => t.FromAccount!.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return list.Select(t => new TransferResponse(
                t.Id,
                t.FromAccountId,
                t.ToIban,
                t.Amount,
                t.Description,
                t.Status.ToString(),
                t.CreatedAt
            )).ToList();
        }

        public async Task<Transfer> GetOwnedAsync(int userId, int transferId)
        {
            var t = await _db.Transfers
                .Include(x => x.FromAccount)
                .FirstOrDefaultAsync(x => x.Id == transferId);

            if (t is null) throw new InvalidOperationException("Transfer not found");
            if (t.FromAccount is null || t.FromAccount.UserId != userId) throw new InvalidOperationException("Not allowed");

            return t;
        }
    }
}
