using Microsoft.Data.SqlClient;
using PaymentService2.Data;
using PaymentService2.Models;

namespace PaymentService2.Services;

public interface IVoucherService
{
    Task<List<Voucher>> GetVouchersAsync();
    Task<VoucherApplyResult> ApplyVoucherAsync(string code, decimal orderTotal);
    Task UpdateVoucherUsageAsync(string code);
}

public class VoucherService : IVoucherService
{
    private readonly SqlHelper _sql;

    public VoucherService(SqlHelper sql)
    {
        _sql = sql;
    }

    public async Task<List<Voucher>> GetVouchersAsync()
    {
        return await _sql.ExecuteReaderAsync(
            "SP_GetVouchers",
            MapVoucher
        );
    }

    public async Task<VoucherApplyResult> ApplyVoucherAsync(string code, decimal orderTotal)
    {
        var result = await _sql.ExecuteReaderSingleAsync(
            "SP_ApplyVoucher",
            reader => new VoucherApplyResult
            {
                Success = SqlHelper.GetBool(reader, "Success"),
                Message = SqlHelper.GetString(reader, "Message"),
                Discount = SqlHelper.GetDecimal(reader, "Discount")
            },
            new SqlParameter("@Code", code),
            new SqlParameter("@OrderTotal", orderTotal)
        );

        return result ?? new VoucherApplyResult { Success = false, Message = "Failed to apply voucher" };
    }

    public async Task UpdateVoucherUsageAsync(string code)
    {
        await _sql.ExecuteNonQueryAsync(
            "SP_UpdateVoucherUsage",
            new SqlParameter("@Code", code)
        );
    }

    private static Voucher MapVoucher(SqlDataReader reader)
    {
        return new Voucher
        {
            Id = SqlHelper.GetInt(reader, "Id"),
            Code = SqlHelper.GetString(reader, "Code"),
            Description = SqlHelper.GetValue<string>(reader, "Description"),
            DiscountType = SqlHelper.GetString(reader, "DiscountType"),
            DiscountValue = SqlHelper.GetDecimal(reader, "DiscountValue"),
            MinOrderAmount = SqlHelper.GetDecimal(reader, "MinOrderAmount"),
            MaxDiscount = SqlHelper.GetValue<decimal?>(reader, "MaxDiscount"),
            UsageLimit = SqlHelper.GetValue<int?>(reader, "UsageLimit"),
            UsedCount = SqlHelper.GetInt(reader, "UsedCount"),
            ExpiresAt = SqlHelper.GetNullableDateTime(reader, "ExpiresAt"),
            IsActive = SqlHelper.GetBool(reader, "IsActive"),
            CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt")
        };
    }
}
