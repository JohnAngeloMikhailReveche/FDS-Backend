using PaymentService2.Models;

namespace PaymentService2.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(string userId, CreateOrderRequest request);
    Task<Order?> GetOrderAsync(string orderId);
    Task<List<Order>> GetOrdersAsync(string userId, int limit = 10);
    Task<Order> CompleteOrderAsync(string orderId);
    Task<Order> PayOrderAsync(string userId, string orderId, PayOrderRequest request);
}
