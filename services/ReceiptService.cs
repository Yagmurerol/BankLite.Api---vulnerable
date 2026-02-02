using BankLite.Api.Data;
using BankLite.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;

namespace BankLite.Api.Services
{
    public class ReceiptService
    {
        private readonly BankLiteDbContext _db;
        public ReceiptService(BankLiteDbContext db) => _db = db;



        public async Task<Receipt> CreateForTransferAsync(Transfer t)
        {
            // Tekrar üretme
            var existing = await _db.Receipts.FirstOrDefaultAsync(r => r.TransferId == t.Id);
            if (existing is not null) return existing;

            var receiptNo = $"RCPT-{DateTime.UtcNow:yyyyMMdd}-{t.Id:D6}";
            var payload = new
            {
                receiptNo,
                transferId = t.Id,
                fromAccountId = t.FromAccountId,
                toIban = t.ToIban,
                amount = t.Amount,
                description = t.Description,
                completedAt = t.CompletedAt,
                createdAt = DateTime.UtcNow
            };

            var r = new Receipt
            {
                TransferId = t.Id,
                ReceiptNo = receiptNo,
                ContentJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true })
            };

            _db.Receipts.Add(r);
            await _db.SaveChangesAsync();
            return r;
        }

        public async Task<Receipt?> GetByTransferIdAsync(int transferId)
        {
            return await _db.Receipts.FirstOrDefaultAsync(r => r.TransferId == transferId);
        }
    }
}
