using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PaymentService2.Data;
using PaymentService2.Models;

namespace PaymentService2.Services;

public class OrderService : IOrderService
{
    private readonly SqlHelper _sql;
    private readonly IWalletService _walletService;
    private readonly IVoucherService _voucherService;
    private readonly IPaymentProvider _paymentProvider;
    private readonly string _frontendUrl;

    public OrderService(SqlHelper sql, IWalletService walletService, IVoucherService voucherService, IPaymentProvider paymentProvider, IConfiguration configuration)
    {
        _sql = sql;
        _walletService = walletService;
        _voucherService = voucherService;
        _paymentProvider = paymentProvider;
        _frontendUrl = configuration["FRONTEND_URL"] ?? "http://localhost:5173"; // Default to Vite port
    }

    public async Task<Order> CreateOrderAsync(string userId, CreateOrderRequest request)
    {
        var subtotal = request.Items.Sum(i => i.Price * i.Quantity);
        decimal discountAmount = 0;
        decimal voucherDiscount = 0;
        decimal coinsDiscount = 0;

        // 1. Apply Voucher
        if (!string.IsNullOrEmpty(request.VoucherCode))
        {
            var vResult = await _voucherService.ApplyVoucherAsync(request.VoucherCode, subtotal);
            if (vResult.Success) 
            {
                voucherDiscount = vResult.Discount;
                discountAmount += voucherDiscount;
                await _voucherService.UpdateVoucherUsageAsync(request.VoucherCode);
            }
        }

        var finalAmount = subtotal - discountAmount;
        var paymentMethod = request.PaymentMethod?.ToLower() ?? "wallet";

        // 2. Apply Coins (Calculation only)
        int coinsUsed = 0;
        if (request.CoinsToUse > 0)
        {
            var coinsToApply = (int)Math.Min(request.CoinsToUse, Math.Ceiling(finalAmount));
            if (coinsToApply > 0)
            {
                coinsUsed = coinsToApply;
                coinsDiscount = coinsToApply;
                discountAmount += coinsDiscount;
                finalAmount -= coinsDiscount;
            }
        }

        // 3. Create Order (SP)
        // SP_CreateOrder generates OrderId.
        var orderIdObj = await _sql.ExecuteScalarAsync<string>(
            "SP_CreateOrder",
            new SqlParameter("@UserId", userId),
            new SqlParameter("@Amount", subtotal),
            new SqlParameter("@PaymentMethod", paymentMethod),
            new SqlParameter("@VoucherCode", (object?)request.VoucherCode ?? DBNull.Value),
            new SqlParameter("@VoucherDiscount", voucherDiscount),
            new SqlParameter("@CoinsUsed", coinsUsed),
            new SqlParameter("@CoinsDiscount", coinsDiscount),
            new SqlParameter("@Branch", (object?)request.Branch ?? DBNull.Value)
        );

        string orderId = orderIdObj ?? throw new Exception("Failed to create order ID");

        // 4. Add Order Items (SP)
        foreach (var item in request.Items)
        {
            await _sql.ExecuteNonQueryAsync(
                "SP_AddOrderItem",
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@Name", item.Name),
                new SqlParameter("@Quantity", item.Quantity),
                new SqlParameter("@Price", item.Price)
            );
        }

        // 5. Use Coins (Wallet Transaction)
        if (coinsUsed > 0)
        {
            await _walletService.UseCoinsAsync(userId, coinsUsed, orderId, $"Coins used for order {orderId}");
        }

        // 6. Process Payment
        if (paymentMethod == "wallet")
        {
            await _walletService.DeductBalanceAsync(userId, finalAmount, orderId, $"Order - {request.Branch}");
            await CompleteOrderAsync(orderId);
        }
        else
        {
            // External Payment with Redirect URL
            var redirectUrl = $"{_frontendUrl}/checkout/{orderId}?payment_return=true";
            var paymentLink = await _paymentProvider.CreatePaymentLinkAsync(finalAmount, $"Order {orderId}", orderId, redirectUrl);
            
            string? linkId = paymentLink.Data?.Id;
            string? url = paymentLink.Data?.Url;
            string? checkoutSessionId = paymentLink.CheckoutId; 

            await _sql.ExecuteNonQueryAsync(
                "SP_UpdateOrderPaymentData",
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@PaymentUrl", (object?)url ?? DBNull.Value),
                new SqlParameter("@PaymentLinkId", (object?)linkId ?? DBNull.Value),
                new SqlParameter("@CheckoutSessionId", (object?)checkoutSessionId ?? DBNull.Value)
            );
        }

        return await GetOrderAsync(orderId) ?? throw new Exception("Order created but not found");
    }

    public async Task<Order?> GetOrderAsync(string orderId)
    {
        var order = await _sql.ExecuteReaderSingleAsync(
            "SP_GetOrder",
            MapOrderInternal,
            new SqlParameter("@OrderId", orderId)
        );

        if (order != null)
        {
            order.Items = await _sql.ExecuteReaderAsync(
                "SP_GetOrderItems",
                MapOrderItem,
                new SqlParameter("@OrderId", orderId)
            );
        }

        return order;
    }

    public async Task<List<Order>> GetOrdersAsync(string userId, int limit = 10)
    {
        var orders = await _sql.ExecuteReaderAsync(
            "SP_GetOrdersByUser",
            MapOrderInternal,
            new SqlParameter("@UserId", userId)
        );

        foreach (var order in orders)
        {
            order.Items = await _sql.ExecuteReaderAsync(
                "SP_GetOrderItems",
                MapOrderItem,
                new SqlParameter("@OrderId", order.Id)
            );
        }
        
        return orders.Take(limit).ToList();
    }

    public async Task<Order> CompleteOrderAsync(string orderId)
    {
        await _sql.ExecuteNonQueryAsync("SP_CompleteOrder", new SqlParameter("@OrderId", orderId));
        return await GetOrderAsync(orderId) ?? throw new Exception("Order not found after completion");
    }

    public async Task<Order> PayOrderAsync(string userId, string orderId, PayOrderRequest request)
    {
        var order = await GetOrderAsync(orderId);
        if (order == null) throw new InvalidOperationException("Order not found");
        if (order.UserId != userId) throw new InvalidOperationException("Order does not belong to the current user");
        if (order.Status != "pending") throw new InvalidOperationException($"Order is not pending (current: {order.Status})");

        var subtotal = order.Items.Sum(i => i.Price * i.Quantity);
        decimal voucherDiscount = 0;
        decimal coinsDiscount = 0;
        decimal discountAmount = 0;

        if (!string.IsNullOrEmpty(request.VoucherCode))
        {
            var vResult = await _voucherService.ApplyVoucherAsync(request.VoucherCode, subtotal);
            if (vResult.Success) 
            {
                voucherDiscount = vResult.Discount;
                discountAmount += voucherDiscount;
                await _voucherService.UpdateVoucherUsageAsync(request.VoucherCode);
            }
        }

        var finalAmount = subtotal - discountAmount;

        int coinsUsed = 0;
        if (request.CoinsToUse > 0)
        {
            var coinsToApply = (int)Math.Min(request.CoinsToUse, Math.Ceiling(finalAmount));
            if (coinsToApply > 0)
            {
                coinsUsed = coinsToApply;
                coinsDiscount = coinsToApply;
                discountAmount += coinsDiscount;
                finalAmount -= coinsDiscount;
                
                await _walletService.UseCoinsAsync(userId, coinsUsed, order.Id, $"Coins used for order {order.Id}");
            }
        }

        var paymentMethod = request.PaymentMethod?.ToLower() ?? "wallet";
        
        string status = "pending";
        string? paymentUrl = null;
        string? paymentLinkId = null;
        string? checkoutSessionId = null;

        if (paymentMethod == "wallet")
        {
            await _walletService.DeductBalanceAsync(userId, finalAmount, order.Id, $"Order - {order.Branch}");
            status = "completed";
        }
        else
        {
            var redirectUrl = $"{_frontendUrl}/checkout/{orderId}?payment_return=true";
            var paymentLink = await _paymentProvider.CreatePaymentLinkAsync(finalAmount, $"Order {order.Id}", order.Id, redirectUrl);
            paymentUrl = paymentLink.Data?.Url;
            paymentLinkId = paymentLink.Data?.Id;
            checkoutSessionId = paymentLink.CheckoutId;
        }

        await _sql.ExecuteNonQueryAsync(
            "SP_UpdateOrder",
            new SqlParameter("@OrderId", orderId),
            new SqlParameter("@PaymentMethod", paymentMethod),
            new SqlParameter("@VoucherCode", (object?)request.VoucherCode ?? DBNull.Value),
            new SqlParameter("@VoucherDiscount", voucherDiscount),
            new SqlParameter("@CoinsDiscount", coinsDiscount),
            new SqlParameter("@FinalAmount", finalAmount),
            new SqlParameter("@Status", status),
            new SqlParameter("@PaymentUrl", (object?)paymentUrl ?? DBNull.Value),
            new SqlParameter("@PaymentLinkId", (object?)paymentLinkId ?? DBNull.Value)
        );

        if (paymentMethod == "wallet")
        {
            await CompleteOrderAsync(orderId);
        }

        if (!string.IsNullOrEmpty(checkoutSessionId))
        {
            await _sql.ExecuteNonQueryAsync(
                "SP_UpdateOrderPaymentData",
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@PaymentUrl", (object?)paymentUrl ?? DBNull.Value),
                new SqlParameter("@PaymentLinkId", (object?)paymentLinkId ?? DBNull.Value),
                new SqlParameter("@CheckoutSessionId", checkoutSessionId)
            );
        }

        return await GetOrderAsync(orderId) ?? throw new Exception("Order not found");
    }

    private static Order MapOrderInternal(SqlDataReader reader)
    {
         var order = new Order
        {
            Id = SqlHelper.GetString(reader, "Id"),
            UserId = SqlHelper.GetString(reader, "UserId"),
            Amount = SqlHelper.GetDecimal(reader, "Amount"),
            Status = SqlHelper.GetString(reader, "Status"),
            PaymentMethod = SqlHelper.GetValue<string>(reader, "PaymentMethod"),
            PaymentStatus = SqlHelper.GetValue<string>(reader, "PaymentStatus") ?? "pending",
            VoucherCode = SqlHelper.GetValue<string>(reader, "VoucherCode"),
            VoucherDiscount = SqlHelper.GetDecimal(reader, "VoucherDiscount"),
            CoinsUsed = SqlHelper.GetInt(reader, "CoinsUsed"),
            CoinsDiscount = SqlHelper.GetDecimal(reader, "CoinsDiscount"),
            FinalAmount = SqlHelper.GetDecimal(reader, "FinalAmount"),
            Branch = SqlHelper.GetValue<string>(reader, "Branch"),
            CreatedAt = SqlHelper.GetDateTime(reader, "CreatedAt"),
            CompletedAt = SqlHelper.HasColumn(reader, "CompletedAt") ? SqlHelper.GetNullableDateTime(reader, "CompletedAt") : null,
            PaymentUrl = SqlHelper.HasColumn(reader, "PaymentUrl") ? SqlHelper.GetValue<string>(reader, "PaymentUrl") : null,
            PaymentLinkId = SqlHelper.HasColumn(reader, "PaymentLinkId") ? SqlHelper.GetValue<string>(reader, "PaymentLinkId") : null,
            CheckoutSessionId = SqlHelper.HasColumn(reader, "CheckoutSessionId") ? SqlHelper.GetValue<string>(reader, "CheckoutSessionId") : null,
            Items = new List<OrderItem>()
        };
        return order;
    }

    private static OrderItem MapOrderItem(SqlDataReader reader)
    {
        return new OrderItem
        {
            Id = SqlHelper.GetInt(reader, "Id"),
            OrderId = SqlHelper.GetString(reader, "OrderId"),
            Name = SqlHelper.GetString(reader, "Name"),
            Quantity = SqlHelper.GetInt(reader, "Quantity"),
            Price = SqlHelper.GetDecimal(reader, "Price")
        };
    }
}