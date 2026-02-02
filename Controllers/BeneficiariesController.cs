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
    public class BeneficiariesController : ControllerBase
    {
        private readonly BeneficiaryService _svc;
        private readonly ILogger<BeneficiariesController> _logger;

        public BeneficiariesController(BeneficiaryService svc, ILogger<BeneficiariesController> logger)
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
        public async Task<ActionResult<List<BeneficiaryDto>>> List()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Invalid or missing user id claim." });

            var res = await _svc.ListAsync(userId);
            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult<BeneficiaryDto>> Add([FromBody] AddBeneficiaryRequest req)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Invalid or missing user id claim." });

            try
            {
                var res = await _svc.AddAsync(userId, req);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                // ✅ SECURITY: Log but don't expose details
                _logger.LogWarning(ex, "Beneficiary add failed for user {UserId}", userId);
                return BadRequest(new { error = "Unable to add beneficiary. Please check your input." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding beneficiary for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while adding the beneficiary." });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Remove(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Invalid or missing user id claim." });

            try
            {
                await _svc.RemoveAsync(userId, id);
                return Ok(new { ok = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing beneficiary for user {UserId}, beneficiary {BeneficiaryId}", userId, id);
                return StatusCode(500, new { error = "An error occurred while removing the beneficiary." });
            }
        }
    }
}
