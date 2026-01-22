using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PaymentService2.Models;
using PaymentService2.Services;

namespace PaymentService2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    private string ResolveUserId(string? userId)
    {
        if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin")) return userId;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(claim)) return claim;
        return "user_001";
    }

    [HttpGet]
    public async Task<ActionResult<WalletResponse>> GetWallet([FromQuery] string? userId = null)
    {
        userId ??= ResolveUserId(userId);
        var wallet = await _walletService.GetWalletAsync(userId);
        return Ok(new WalletResponse { Success = true, Data = wallet });
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<WalletResponse>> GetWalletByPath(string userId)
    {
        if (!User.IsInRole("Admin"))
        {
            var callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(callerId) || callerId != userId) return Forbid();
        }

        var wallet = await _walletService.GetWalletAsync(userId);
        return Ok(new WalletResponse { Success = true, Data = wallet });
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<TransactionListResponse>> GetTransactions(
        [FromQuery] string? userId = null,
        [FromQuery] int limit = 10)
    {
        userId ??= ResolveUserId(userId);
        var transactions = await _walletService.GetTransactionsAsync(userId, limit);
        return Ok(new TransactionListResponse
        {
            Success = true,
            Data = transactions,
            Total = transactions.Count
        });
    }

    [HttpPut("{userId}/balance")]
    public async Task<ActionResult<WalletResponse>> UpdateBalance(string userId, [FromBody] UpdateBalanceRequest request)
    {
        try
        {
            if (!User.IsInRole("Admin")) return Forbid();

            var wallet = await _walletService.AddBalanceAsync(userId, request.Amount, null, request.Description, "topup");
            return Ok(new WalletResponse { Success = true, Data = wallet });
        }
        catch (Exception ex)
        {
            return BadRequest(new WalletResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("{userId}/use-coins")]
    public async Task<ActionResult<WalletResponse>> UseCoins(string userId, [FromBody] UseCoinsRequest request)
    {
        try
        {
            if (!User.IsInRole("Admin")) return Forbid();

            var wallet = await _walletService.UseCoinsAsync(userId, request.Amount, null, "Coins used");
            return Ok(new WalletResponse { Success = true, Data = wallet });
        }
        catch (Exception ex)
        {
            return BadRequest(new WalletResponse { Success = false, Message = ex.Message });
        }
    }
}

public class UpdateBalanceRequest
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class UseCoinsRequest
{
    public int Amount { get; set; }
}
