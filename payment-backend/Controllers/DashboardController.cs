using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PaymentService2.Services;
using PaymentService2.Models;


namespace PaymentService2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IWalletService _walletService;

    public DashboardController(IWalletService walletService)
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

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats>> GetStats([FromQuery] string? userId = null)
    {
        userId ??= ResolveUserId(userId);
        
        try
        {
            var wallet = await _walletService.GetWalletAsync(userId);
            var transactions = await _walletService.GetTransactionsAsync(userId, 5);

            return Ok(new DashboardStats
            {
                UserId = userId,
                WalletBalance = wallet.Balance,
                Coins = wallet.Coins,
                RecentTransactions = transactions,
                RecentTransactionCount = transactions.Count,
                LastUpdated = wallet.LastUpdated
            });
        }
        catch (Exception ex)
        {
            return Ok(new DashboardStats
            {
                UserId = userId,
                WalletBalance = 0,
                Coins = 0,
                RecentTransactions = new(),
                RecentTransactionCount = 0,
                ErrorMessage = ex.Message
            });
        }
    }
}

public class DashboardStats
{
    public string UserId { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }
    public int Coins { get; set; }
    public List<Transaction> RecentTransactions { get; set; } = new();
    public int RecentTransactionCount { get; set; }
    public DateTime? LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}
