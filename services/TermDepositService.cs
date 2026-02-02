using BankLite.Api.Data;
using BankLite.Api.Dtos;
using BankLite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLite.Api.Services
{
    public class TermDepositService
    {
        private readonly BankLiteDbContext _db;
        private readonly AccountService _accounts;

        public TermDepositService(BankLiteDbContext db, AccountService accounts)
        {
            _db = db;
            _accounts = accounts;
        }

        public async Task<DepositResponse> OpenAsync(int userId, OpenDepositRequest req)
        {
            if (req.Principal <= 0) throw new InvalidOperationException("Principal must be > 0");
            if (req.Days <= 0) throw new InvalidOperationException("Days must be > 0");
            if (req.Rate <= 0) throw new InvalidOperationException("Rate must be > 0");

            var from = await _accounts.GetOwnedAsync(userId, req.FromAccountId);
            var payout = await _accounts.GetOwnedAsync(userId, req.PayoutAccountId);

            if (from.IsClosed || payout.IsClosed) throw new InvalidOperationException("Account closed");
            if (from.Currency != payout.Currency) throw new InvalidOperationException("Currency mismatch");
            if (from.Balance < req.Principal) throw new InvalidOperationException("Yetersiz bakiye");

            using var trx = await _db.Database.BeginTransactionAsync();
            await _db.Entry(from).ReloadAsync();

            from.Balance -= req.Principal;

            var d = new TermDeposit
            {
                UserId = userId,
                FromAccountId = from.Id,
                PayoutAccountId = payout.Id,
                Principal = req.Principal,
                Rate = req.Rate,
                StartDate = DateTime.UtcNow,
                MaturityDate = DateTime.UtcNow.AddDays(req.Days),
                Status = TermDepositStatus.Open
            };

            _db.TermDeposits.Add(d);
            await _db.SaveChangesAsync();
            await trx.CommitAsync();

            return new DepositResponse(d.Id, d.FromAccountId, d.PayoutAccountId, d.Principal, d.Rate, d.StartDate, d.MaturityDate, d.Status.ToString());
        }

        public async Task<DepositResponse> CloseAsync(int userId, int depositId)
        {
            var d = await _db.TermDeposits.FirstOrDefaultAsync(x => x.Id == depositId && x.UserId == userId);
            if (d is null) throw new InvalidOperationException("Deposit not found");
            if (d.Status == TermDepositStatus.Closed)
                return new DepositResponse(d.Id, d.FromAccountId, d.PayoutAccountId, d.Principal, d.Rate, d.StartDate, d.MaturityDate, d.Status.ToString());

            var payout = await _accounts.GetOwnedAsync(userId, d.PayoutAccountId);
            if (payout.IsClosed) throw new InvalidOperationException("Payout account closed");

            // MVP faiz hesabı: simple interest (principal * rate * days/365)
            var totalDays = (DateTime.UtcNow.Date - d.StartDate.Date).TotalDays;
            if (totalDays < 0) totalDays = 0;
            var interest = d.Principal * d.Rate * (decimal)(totalDays / 365.0);
            var payoutAmount = d.Principal + interest;

            using var trx = await _db.Database.BeginTransactionAsync();

            payout.Balance += payoutAmount;

            d.Status = TermDepositStatus.Closed;
            d.ClosedAt = DateTime.UtcNow;
            d.PayoutAmount = payoutAmount;

            await _db.SaveChangesAsync();
            await trx.CommitAsync();

            return new DepositResponse(d.Id, d.FromAccountId, d.PayoutAccountId, d.Principal, d.Rate, d.StartDate, d.MaturityDate, d.Status.ToString());
        }

        public async Task<List<DepositResponse>> ListAsync(int userId)
        {
            var list = await _db.TermDeposits
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();

            return list.Select(d => new DepositResponse(d.Id, d.FromAccountId, d.PayoutAccountId, d.Principal, d.Rate, d.StartDate, d.MaturityDate, d.Status.ToString())).ToList();
        }
    }
}
