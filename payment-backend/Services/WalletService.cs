using Microsoft.Data.SqlClient;
using PaymentService2.Data;
using PaymentService2.Models;

namespace PaymentService2.Services;

public interface IWalletService
{
    Task<Wallet> GetWalletAsync(string userId);
    Task<Wallet> AddBalanceAsync(string userId, decimal amount, string? referenceId = null, string? description = null, string transactionType = "topup");
    Task<Wallet> DeductBalanceAsync(string userId, decimal amount, string? referenceId = null, string? description = null);
    Task<Wallet> UseCoinsAsync(string userId, int coinsToUse, string? referenceId = null, string? description = null);
    Task<List<Transaction>> GetTransactionsAsync(string userId, int limit = 10);
}

public class WalletService : IWalletService
{
    private readonly SqlHelper _sql;

    public WalletService(SqlHelper sql)
    {
        _sql = sql;
    }

    public async Task<Wallet> GetWalletAsync(string userId)
    {
        var wallet = await _sql.ExecuteReaderSingleAsync(
            "SP_GetWallet",
            MapWallet,
            new SqlParameter("@UserId", userId)
        );

        return wallet ?? new Wallet { UserId = userId, Balance = 0, Coins = 0 };
    }

    public async Task<Wallet> AddBalanceAsync(string userId, decimal amount, string? referenceId = null, string? description = null, string transactionType = "topup")
    {
        var wallet = await _sql.ExecuteReaderSingleAsync(
            "SP_AddBalance",
            MapWallet,
            new SqlParameter("@UserId", userId),
            new SqlParameter("@Amount", amount),
            new SqlParameter("@ReferenceId", (object?)referenceId ?? DBNull.Value),
            new SqlParameter("@Description", (object?)description ?? DBNull.Value),
            new SqlParameter("@TransactionType", transactionType)
        );

        return wallet ?? throw new InvalidOperationException("Failed to add balance");
    }

    public async Task<Wallet> DeductBalanceAsync(string userId, decimal amount, string? referenceId = null, string? description = null)
    {
        var wallet = await _sql.ExecuteReaderSingleAsync(
            "SP_DeductBalance",
            MapWallet,
            new SqlParameter("@UserId", userId),
            new SqlParameter("@Amount", amount),
            new SqlParameter("@ReferenceId", (object?)referenceId ?? DBNull.Value),
            new SqlParameter("@Description", (object?)description ?? DBNull.Value)
        );

        return wallet ?? throw new InvalidOperationException("Failed to deduct balance");
    }

    public async Task<Wallet> UseCoinsAsync(string userId, int coinsToUse, string? referenceId = null, string? description = null)
    {
        var wallet = await _sql.ExecuteReaderSingleAsync(
            "SP_UseCoins",
            MapWallet,
            new SqlParameter("@UserId", userId),
            new SqlParameter("@CoinsToUse", coinsToUse),
            new SqlParameter("@ReferenceId", (object?)referenceId ?? DBNull.Value),
            new SqlParameter("@Description", (object?)description ?? DBNull.Value)
        );

        return wallet ?? throw new InvalidOperationException("Failed to use coins");
    }

    public async Task<List<Transaction>> GetTransactionsAsync(string userId, int limit = 10)
    {
        return await _sql.ExecuteReaderAsync(
            "SP_GetTransactions",
            MapTransaction,
            new SqlParameter("@UserId", userId),
            new SqlParameter("@Limit", limit)
        );
    }

    private static Wallet MapWallet(SqlDataReader reader)
    {
        return new Wallet
        {
            Id = SqlHelper.GetInt(reader, "Id"),
            UserId = SqlHelper.GetString(reader, "UserId"),
            Balance = SqlHelper.GetDecimal(reader, "Balance"),
            Coins = SqlHelper.GetInt(reader, "Coins"),
            LastUpdated = SqlHelper.GetDateTime(reader, "LastUpdated")
        };
    }

    private static Transaction MapTransaction(SqlDataReader reader)
    {
        return new Transaction
        {
            Id = SqlHelper.GetString(reader, "Id"),
            UserId = SqlHelper.GetString(reader, "UserId"),
            Type = SqlHelper.GetString(reader, "Type"),
            Amount = SqlHelper.GetDecimal(reader, "Amount"),
            Description = SqlHelper.GetValue<string>(reader, "Description"),
            ReferenceId = SqlHelper.GetValue<string>(reader, "ReferenceId"),
            CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt")
        };
    }
}
