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
    public class TransfersController : ControllerBase
    {
        private readonly TransferService _svc;
        private readonly ReceiptService _receipts;
        private readonly ILogger<TransfersController> _logger;

        public TransfersController(TransferService svc, ReceiptService receipts, ILogger<TransfersController> logger)
        {
            _svc = svc;
            _receipts = receipts;
            _logger = logger;
        }

        private int UserId => int.Parse(User.FindFirst("id")?.Value
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<List<TransferResponse>>> List()
        {
            var res = await _svc.ListAsync(UserId);
            return Ok(res);
        }

        [HttpPost("initiate")]
        public async Task<ActionResult<InitiateTransferResponse>> Initiate(InitiateTransferRequest req)
        {
            try
            {
                var res = await _svc.InitiateAsync(UserId, req);
                return Ok(res);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                // ✅ SECURITY: Log details but return generic message
                _logger.LogWarning(ex, "Transfer initiation failed for user {UserId}", UserId);
                return BadRequest(new { error = "Unable to process transfer. Please check your account details." });
            }
            catch (Exception ex)
            {
                // ✅ SECURITY: Never expose internal error details
                _logger.LogError(ex, "Unexpected error during transfer initiation for user {UserId}", UserId);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        // ⚠️ REMOVED: Unsafe endpoint that allowed unauthorized transfers removed
        // Use the secure /initiate endpoint instead, which validates fromAccountId ownership

        [HttpPost("{id:int}/confirm")]
        public async Task<ActionResult> Confirm(int id, ConfirmTransferRequest req)
        {
            try
            {
                await _svc.CompleteAsync(UserId, id, req.OtpCode);
                return Ok(new { ok = true });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Transfer confirmation failed for user {UserId}, transfer {TransferId}", UserId, id);
                return BadRequest(new { error = "Invalid OTP code or transfer not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during transfer confirmation for user {UserId}", UserId);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpGet("{id:int}/receipt")]
        public async Task<ActionResult> Receipt(int id)
        {
            try
            {
                var t = await _svc.GetOwnedAsync(UserId, id);
                var r = await _receipts.GetByTransferIdAsync(t.Id);
                if (r is null) return NotFound(new { error = "Receipt not found" });

                return Ok(new
                {
                    r.ReceiptNo,
                    r.CreatedAt,
                    r.ContentJson
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving receipt for user {UserId}, transfer {TransferId}", UserId, id);
                return StatusCode(500, new { error = "An error occurred while retrieving the receipt." });
            }
        }
    }
}
