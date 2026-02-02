using BankLite.Api.Dtos;
using BankLite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankLite.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _svc;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(AccountService svc, ILogger<AccountsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var userIdStr = User.FindFirst("id")?.Value
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out userId);
        }

        [HttpGet]
        public async Task<ActionResult<List<AccountResponse>>> List()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Invalid or missing user id claim." });

            var res = await _svc.ListAsync(userId);
            return Ok(res);
        }

        // ⚠️ FIXED: IDOR prevented - removed unsafe endpoint that allowed access to other users' accounts
        // Users can only access their own accounts via the /accounts endpoint (which validates ownership)

        [HttpPost]
        public async Task<ActionResult<AccountResponse>> Create([FromBody] CreateAccountRequest req)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Invalid or missing user id claim." });

            try
            {
                var res = await _svc.CreateAsync(userId, req);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                // ✅ SECURITY: Log but don't expose details
                _logger.LogWarning(ex, "Account creation failed for user {UserId}", userId);
                return BadRequest(new { error = "Unable to create account. Please check your input." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating account for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while creating the account." });
            }
        }

        [HttpPost("{id:int}/close")]
        public async Task<ActionResult> Close(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Invalid or missing user id claim." });

            try
            {
                await _svc.CloseAsync(userId, id);
                return Ok(new { ok = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Account close failed for user {UserId}, account {AccountId}", userId, id);
                return BadRequest(new { error = "Unable to close account." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error closing account for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while closing the account." });
            }
        }
    }
}
