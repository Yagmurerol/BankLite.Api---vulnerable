using BankLite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankLite.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReceiptsController : ControllerBase
    {
        private readonly ReceiptService _receipts;
        private readonly TransferService _transfers;
        private readonly ILogger<ReceiptsController> _logger;

        public ReceiptsController(ReceiptService receipts, TransferService transfers, ILogger<ReceiptsController> logger)
        {
            _receipts = receipts;
            _transfers = transfers;
            _logger = logger;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /api/Receipts/{transferId}
        [HttpGet("{transferId:int}")]
        public async Task<ActionResult> GetByTransferId(int transferId)
        {
            try
            {
                // Ownership kontrol
                var transfer = await _transfers.GetOwnedAsync(UserId, transferId);
                
                var receipt = await _receipts.GetByTransferIdAsync(transfer.Id);
                if (receipt == null)
                    return NotFound(new { error = "Receipt not found" });

                return Ok(new
                {
                    receiptNo = receipt.ReceiptNo,
                    createdAt = receipt.CreatedAt,
                    transferId = receipt.TransferId,
                    contentJson = receipt.ContentJson
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                // ✅ SECURITY: Log but don't expose details
                _logger.LogError(ex, "Error retrieving receipt for user {UserId}, transfer {TransferId}", UserId, transferId);
                return StatusCode(500, new { error = "An error occurred while retrieving the receipt." });
            }
        }
    }
}
