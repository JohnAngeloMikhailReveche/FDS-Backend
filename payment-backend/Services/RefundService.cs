using Microsoft.Data.SqlClient;
using PaymentService2.Data;
using PaymentService2.Models;

namespace PaymentService2.Services;

public interface IRefundService
{
    Task<RefundRequest> CreateRefundAsync(string userId, string? orderId, decimal amount, string? reason, string? category, string? customerName, string? customerEmail, string? customerPhone, string? photoPath = null);
    Task<List<RefundRequest>> GetRefundsAsync(string? userId = null, string? status = null);
    Task<RefundRequest?> GetRefundAsync(string refundId);
    Task<RefundRequest> ReviewRefundAsync(string refundId, string action, string? adminNotes, string? rejectionReason, string? reviewedBy);
    Task<RefundRequest> ProcessRefundToWalletAsync(string refundId);
}

public class RefundService : IRefundService
{
    private readonly SqlHelper _sql;

    public RefundService(SqlHelper sql)
    {
        _sql = sql;
    }

    public async Task<RefundRequest> CreateRefundAsync(string userId, string? orderId, decimal amount, string? reason, string? category, string? customerName, string? customerEmail, string? customerPhone, string? photoPath = null)
    {
        var refund = await _sql.ExecuteReaderSingleAsync(
            "SP_CreateRefund",
            MapRefund,
            new SqlParameter("@UserId", userId),
            new SqlParameter("@OrderId", (object?)orderId ?? DBNull.Value),
            new SqlParameter("@Amount", amount),
            new SqlParameter("@Reason", (object?)reason ?? DBNull.Value),
            new SqlParameter("@Category", (object?)category ?? DBNull.Value),
            new SqlParameter("@CustomerName", (object?)customerName ?? DBNull.Value),
            new SqlParameter("@CustomerEmail", (object?)customerEmail ?? DBNull.Value),
            new SqlParameter("@CustomerPhone", (object?)customerPhone ?? DBNull.Value),
            new SqlParameter("@PhotoPath", (object?)photoPath ?? DBNull.Value)
        );

        return refund ?? throw new InvalidOperationException("Failed to create refund");
    }

    public async Task<List<RefundRequest>> GetRefundsAsync(string? userId = null, string? status = null)
    {
        return await _sql.ExecuteReaderAsync(
            "SP_GetRefunds",
            MapRefund,
            new SqlParameter("@UserId", (object?)userId ?? DBNull.Value),
            new SqlParameter("@Status", (object?)status ?? DBNull.Value)
        );
    }

    public async Task<RefundRequest?> GetRefundAsync(string refundId)
    {
        var refunds = await GetRefundsAsync();
        return refunds.FirstOrDefault(r => r.Id == refundId);
    }

    public async Task<RefundRequest> ReviewRefundAsync(string refundId, string action, string? adminNotes, string? rejectionReason, string? reviewedBy)
    {
        var refund = await _sql.ExecuteReaderSingleAsync(
            "SP_ReviewRefund",
            MapRefund,
            new SqlParameter("@RefundId", refundId),
            new SqlParameter("@Action", action),
            new SqlParameter("@AdminNotes", (object?)adminNotes ?? DBNull.Value),
            new SqlParameter("@RejectionReason", (object?)rejectionReason ?? DBNull.Value),
            new SqlParameter("@ReviewedBy", (object?)reviewedBy ?? DBNull.Value)
        );

        return refund ?? throw new InvalidOperationException("Failed to review refund");
    }

    public async Task<RefundRequest> ProcessRefundToWalletAsync(string refundId)
    {
        var refund = await _sql.ExecuteReaderSingleAsync(
            "SP_ProcessRefundToWallet",
            MapRefund,
            new SqlParameter("@RefundId", refundId)
        );

        return refund ?? throw new InvalidOperationException("Failed to process refund");
    }

    private static RefundRequest MapRefund(SqlDataReader reader)
    {
        return new RefundRequest
        {
            Id = SqlHelper.GetString(reader, "Id"),
            UserId = SqlHelper.GetString(reader, "UserId"),
            OrderId = SqlHelper.HasColumn(reader, "OrderId") ? SqlHelper.GetValue<string>(reader, "OrderId") : null,
            Amount = SqlHelper.GetDecimal(reader, "Amount"),
            Reason = SqlHelper.HasColumn(reader, "Reason") ? SqlHelper.GetValue<string>(reader, "Reason") : null,
            Category = SqlHelper.HasColumn(reader, "Category") ? SqlHelper.GetValue<string>(reader, "Category") : null,
            Status = SqlHelper.GetString(reader, "Status"),
            CustomerName = SqlHelper.HasColumn(reader, "CustomerName") ? SqlHelper.GetValue<string>(reader, "CustomerName") : null,
            CustomerEmail = SqlHelper.HasColumn(reader, "CustomerEmail") ? SqlHelper.GetValue<string>(reader, "CustomerEmail") : null,
            CustomerPhone = SqlHelper.HasColumn(reader, "CustomerPhone") ? SqlHelper.GetValue<string>(reader, "CustomerPhone") : null,
            PhotoPath = SqlHelper.HasColumn(reader, "PhotoPath") ? SqlHelper.GetValue<string>(reader, "PhotoPath") : null,
            AdminNotes = SqlHelper.HasColumn(reader, "AdminNotes") ? SqlHelper.GetValue<string>(reader, "AdminNotes") : null,
            RejectionReason = SqlHelper.HasColumn(reader, "RejectionReason") ? SqlHelper.GetValue<string>(reader, "RejectionReason") : null,
            ReviewedBy = SqlHelper.HasColumn(reader, "ReviewedBy") ? SqlHelper.GetValue<string>(reader, "ReviewedBy") : null,
            WalletCredited = SqlHelper.HasColumn(reader, "WalletCredited") ? SqlHelper.GetBool(reader, "WalletCredited") : false,
            VoucherCode = SqlHelper.HasColumn(reader, "VoucherCode") ? SqlHelper.GetValue<string>(reader, "VoucherCode") : null,
            VoucherDiscount = SqlHelper.HasColumn(reader, "VoucherDiscount") ? SqlHelper.GetDecimal(reader, "VoucherDiscount") : 0,
            CreatedAt = SqlHelper.HasColumn(reader, "CreatedAt") ? SqlHelper.GetDateTime(reader, "CreatedAt") : DateTime.UtcNow,
            ReviewedAt = SqlHelper.HasColumn(reader, "ReviewedAt") ? SqlHelper.GetNullableDateTime(reader, "ReviewedAt") : null
        };
    }
}
