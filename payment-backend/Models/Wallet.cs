using System.ComponentModel.DataAnnotations;
namespace PaymentService.Models;



public class Wallet
{
    [Key]
    public string UserId { get; set; } = "user_001";
    public decimal Balance { get; set; }
    public int Coins { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class WalletResponse
{
    public bool Success { get; set; }
    public Wallet? Data { get; set; }
    public string? Message { get; set; }
}
