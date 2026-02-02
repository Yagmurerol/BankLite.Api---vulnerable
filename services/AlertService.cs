using BankLite.Api.Data;
using BankLite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BankLite.Api.Services
{
    public class AlertService
    {
        private readonly BankLiteDbContext _db;

        public AlertService(BankLiteDbContext db)
        {
            _db = db;
        }

        public async Task CreateAlertAsync(int userId, AlertType type, string title, string message)
        {
            var alert = new Alert
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Alerts.Add(alert);
            await _db.SaveChangesAsync();
        }

        public async Task CreateTransferAlertAsync(int userId, decimal amount, string toIban, decimal remainingBalance)
        {
            var message = $"{amount:N2} TRY tutarında havale gönderildi. Alıcı: {toIban}. Kalan bakiye: {remainingBalance:N2} TRY";
            await CreateAlertAsync(userId, AlertType.Transaction, "Havale İşlemi", message);

            // Bakiye kontrolleri
            if (remainingBalance < 100)
            {
                await CreateAlertAsync(
                    userId,
                    AlertType.Warning,
                    "⚠️ Kritik Bakiye Uyarısı",
                    $"Hesap bakiyeniz 100 TL'nin altına düştü! Mevcut bakiye: {remainingBalance:N2} TRY"
                );
            }
            else if (remainingBalance < 1000)
            {
                await CreateAlertAsync(
                    userId,
                    AlertType.Warning,
                    "⚠️ Düşük Bakiye Uyarısı",
                    $"Hesap bakiyeniz 1000 TL'nin altına düştü. Mevcut bakiye: {remainingBalance:N2} TRY"
                );
            }
        }

        public async Task<List<Alert>> GetUserAlertsAsync(int userId)
        {
            return await _db.Alerts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int alertId, int userId)
        {
            var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);
            if (alert != null)
            {
                alert.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAlertAsync(int alertId, int userId)
        {
            var alert = await _db.Alerts.FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);
            if (alert != null)
            {
                _db.Alerts.Remove(alert);
                await _db.SaveChangesAsync();
            }
        }

        public async Task ClearAllAlertsAsync(int userId)
        {
            var alerts = await _db.Alerts.Where(a => a.UserId == userId).ToListAsync();
            _db.Alerts.RemoveRange(alerts);
            await _db.SaveChangesAsync();
        }
    }
}
