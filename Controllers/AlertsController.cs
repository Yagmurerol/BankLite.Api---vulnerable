using BankLite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankLite.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AlertsController : ControllerBase
    {
        private readonly AlertService _alertService;

        public AlertsController(AlertService alertService)
        {
            _alertService = alertService;
        }

        private int UserId => int.Parse(User.FindFirst("id")?.Value
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult> GetAlerts()
        {
            var alerts = await _alertService.GetUserAlertsAsync(UserId);
            
            return Ok(alerts.Select(a => new
            {
                a.Id,
                type = a.Type.ToString(),
                a.Title,
                a.Message,
                isRead = a.IsRead,
                createdAt = a.CreatedAt
            }));
        }

        [HttpPost("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            await _alertService.MarkAsReadAsync(id, UserId);
            return Ok(new { ok = true });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAlert(int id)
        {
            await _alertService.DeleteAlertAsync(id, UserId);
            return Ok(new { ok = true });
        }

        [HttpDelete]
        public async Task<ActionResult> ClearAll()
        {
            await _alertService.ClearAllAlertsAsync(UserId);
            return Ok(new { ok = true });
        }
    }
}
