using Microsoft.Data.SqlClient;
using PaymentService2.Data;
using PaymentService2.Models;

namespace PaymentService2.Services;

public interface ITopUpService
{
    Task<TopUp> CreateTopUpAsync(string userId, decimal amount, string paymentMethod);
    Task<TopUp> CompleteTopUpAsync(string topUpId);
    Task<TopUp?> GetTopUpAsync(string topUpId);
    Task UpdateTopUpPaymentUrlAsync(string topUpId, string paymentUrl, string? checkoutSessionId = null);
}

public class TopUpService : ITopUpService
{
    private readonly SqlHelper _sql;

    public TopUpService(SqlHelper sql)
    {
        _sql = sql;
    }

    public async Task<TopUp> CreateTopUpAsync(string userId, decimal amount, string paymentMethod)
    {
        var topUp = await _sql.ExecuteReaderSingleAsync(
            "SP_CreateTopUp",
            MapTopUp,
            new SqlParameter("@UserId", userId),
            new SqlParameter("@Amount", amount),
            new SqlParameter("@PaymentMethod", paymentMethod)
        );

        return topUp ?? throw new InvalidOperationException("Failed to create top-up");
    }

    public async Task<TopUp> CompleteTopUpAsync(string topUpId)
    {
        var topUp = await _sql.ExecuteReaderSingleAsync(
            "SP_CompleteTopUp",
            MapTopUp,
            new SqlParameter("@TopUpId", topUpId)
        );

        return topUp ?? throw new InvalidOperationException("Failed to complete top-up");
    }

    public async Task UpdateTopUpPaymentUrlAsync(string topUpId, string paymentUrl, string? checkoutSessionId = null)
    {
        await _sql.ExecuteNonQueryAsync(
            "SP_UpdateTopUpPaymentUrl",
            new SqlParameter("@TopUpId", topUpId),
            new SqlParameter("@PaymentUrl", paymentUrl),
            new SqlParameter("@CheckoutSessionId", (object?)checkoutSessionId ?? DBNull.Value)
        );
    }

    public async Task<TopUp?> GetTopUpAsync(string topUpId)
    {
        return await _sql.ExecuteReaderSingleAsync(
            "SP_GetTopUp",
            MapTopUp,
            new SqlParameter("@TopUpId", topUpId)
        );
    }

    private static TopUp MapTopUp(SqlDataReader reader)
    {
        return new TopUp
        {
            Id = SqlHelper.GetString(reader, "Id"),
            UserId = SqlHelper.GetString(reader, "UserId"),
            Amount = SqlHelper.GetDecimal(reader, "Amount"),
            Status = SqlHelper.GetString(reader, "Status"),
            PaymentMethod = SqlHelper.GetValue<string>(reader, "PaymentMethod"),
            PaymentUrl = SqlHelper.GetValue<string>(reader, "PaymentUrl"),
            CheckoutSessionId = SqlHelper.HasColumn(reader, "CheckoutSessionId") ? SqlHelper.GetValue<string>(reader, "CheckoutSessionId") : null,
            CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt"),
            // CompletedAt might not be in the result set for SP_CreateTopUp
            CompletedAt = SqlHelper.HasColumn(reader, "CompletedAt") 
                ? SqlHelper.GetNullableDateTime(reader, "CompletedAt") 
                : null
        };
    }
}
