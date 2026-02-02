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
    public class TermDepositsController : ControllerBase
    {
        private readonly TermDepositService _svc;
        private readonly ILogger<TermDepositsController> _logger;

        public TermDepositsController(TermDepositService svc, ILogger<TermDepositsController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<List<DepositResponse>>> List()
        {
            var res = await _svc.ListAsync(UserId);
            return Ok(res);
        }

        [HttpPost("open")]
        public async Task<ActionResult<DepositResponse>> Open(OpenDepositRequest req)
        {
            try
            {
                var res = await _svc.OpenAsync(UserId, req);
                return Ok(res);
            }
            catch (InvalidOperationException ex)
            {
                // ✅ SECURITY: Log but don't expose details
                _logger.LogWarning(ex, "Deposit open failed for user {UserId}", UserId);
                return BadRequest(new { error = "Unable to open deposit. Please check your input." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error opening deposit for user {UserId}", UserId);
                return StatusCode(500, new { error = "An error occurred while opening the deposit." });
            }
        }

        [HttpPost("{id:int}/close")]
        public async Task<ActionResult<DepositResponse>> Close(int id)
        {
            try
            {
                var res = await _svc.CloseAsync(UserId, id);
                return Ok(res);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Deposit close failed for user {UserId}, deposit {DepositId}", UserId, id);
                return BadRequest(new { error = "Unable to close deposit." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error closing deposit for user {UserId}", UserId);
                return StatusCode(500, new { error = "An error occurred while closing the deposit." });
            }
        }
    }
}
