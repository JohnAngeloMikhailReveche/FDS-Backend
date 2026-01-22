using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models.DTO;
using System.Data;

namespace OrderService.Services
{
    public class OrderService
    {
        private readonly OrderDbContext _db;

        public OrderService(OrderDbContext db)
        {
            _db = db;
        }

        // ============================================
        // 1. REQUEST CANCELLATION
        // ============================================
        public async Task<string> RequestCancellationAsync(int orderId, string reason)
        {
            var resultMessage = new SqlParameter
            {
                ParameterName = "@ResultMessage",
                SqlDbType = SqlDbType.NVarChar,
                Size = 500,
                Direction = ParameterDirection.Output
            };

            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_RequestCancellation @OrderId, @Reason, @ResultMessage OUTPUT",
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@Reason", reason),
                resultMessage
            );

            return resultMessage.Value?.ToString() ?? "Unknown error";
        }

        // ============================================
        // 2. PLACE ORDER
        // ============================================
        public async Task<(int orderId, string message)> PlaceOrderAsync(string userId) 
        {
            var newOrderId = new SqlParameter
            {
                ParameterName = "@NewOrderId",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var resultMessage = new SqlParameter
            {
                ParameterName = "@ResultMessage",
                SqlDbType = SqlDbType.NVarChar,
                Size = 500,
                Direction = ParameterDirection.Output
            };

            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_PlaceOrder @UserId, @NewOrderId OUTPUT, @ResultMessage OUTPUT",
                new SqlParameter("@UserId", userId), 
                newOrderId,
                resultMessage
            );

            int orderIdValue = newOrderId.Value != DBNull.Value ? (int)newOrderId.Value : 0;
            string message = resultMessage.Value?.ToString() ?? "Unknown error";

            return (orderIdValue, message);
        }

        // ============================================
        // 3. UPDATE ORDER STATUS
        // ============================================
        public async Task<string> UpdateStatusAsync(int orderId, byte newStatus, string userRole)
        {
            var resultMessage = new SqlParameter
            {
                ParameterName = "@ResultMessage",
                SqlDbType = SqlDbType.NVarChar,
                Size = 500,
                Direction = ParameterDirection.Output
            };

            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_UpdateOrderStatus @OrderId, @NewStatus, @UserRole, @ResultMessage OUTPUT",
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@NewStatus", newStatus),
                new SqlParameter("@UserRole", userRole),
                resultMessage
            );

            return resultMessage.Value?.ToString() ?? "Unknown error";
        }

        // ============================================
        // 4. GET ORDER DETAILS WITH ITEMS
        // ============================================
        public async Task<OrderDetailsDTO?> GetOrderDetailsAsync(int orderId)
        {
            OrderDetailsDTO? orderDetails = null;

            using var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "sp_GetOrderDetails";
            command.CommandType = CommandType.StoredProcedure;

            var orderIdParam = command.CreateParameter();
            orderIdParam.ParameterName = "@OrderId";
            orderIdParam.Value = orderId;
            command.Parameters.Add(orderIdParam);

            using var reader = await command.ExecuteReaderAsync();

            // First result set: Order info
            if (await reader.ReadAsync())
            {
                orderDetails = new OrderDetailsDTO
                {
                    orderId = reader.GetInt32(reader.GetOrdinal("orderId")),
                    subtotal = reader.GetDecimal(reader.GetOrdinal("subtotal")),
                    status = reader.GetByte(reader.GetOrdinal("status")),
                    cancellation_requested = reader.GetBoolean(reader.GetOrdinal("cancellation_requested")),
                    items = new List<OrderItemDetailsDTO>()
                };
            }

            // Second result set: Order items
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    orderDetails?.items.Add(new OrderItemDetailsDTO
                    {
                        imageUrl = reader.IsDBNull(reader.GetOrdinal("imageUrl")) ? null : reader.GetString(reader.GetOrdinal("imageUrl")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                        size = reader.GetString(reader.GetOrdinal("size")),
                        total = reader.GetDecimal(reader.GetOrdinal("total")),
                        specialInstructions = reader.IsDBNull(reader.GetOrdinal("specialInstructions")) ? null : reader.GetString(reader.GetOrdinal("specialInstructions"))
                    });
                }
            }

            return orderDetails;
        }

        // ============================================
        // 5. GET ORDER HISTORY
        // ============================================
        public async Task<List<OrderHistoryDTO>> GetOrderHistoryAsync(string userId, string filter, string sortOrder)
        {
            var orders = new List<OrderHistoryDTO>();

            using var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "sp_GetOrderHistory";
            command.CommandType = CommandType.StoredProcedure;

            var userIdParam = command.CreateParameter();
            userIdParam.ParameterName = "@UserId";
            userIdParam.Value = !string.IsNullOrEmpty(userId) ? (object)userId : DBNull.Value;
            command.Parameters.Add(userIdParam);

            var filterParam = command.CreateParameter();
            filterParam.ParameterName = "@Filter";
            filterParam.Value = filter;
            command.Parameters.Add(filterParam);

            var sortOrderParam = command.CreateParameter();
            sortOrderParam.ParameterName = "@SortOrder";
            sortOrderParam.Value = sortOrder;
            command.Parameters.Add(sortOrderParam);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(new OrderHistoryDTO
                {
                    orders_id = reader.GetInt32(reader.GetOrdinal("orders_id")),
                    users_id = reader.GetString(reader.GetOrdinal("users_id")),
                    total_cost = reader.GetDecimal(reader.GetOrdinal("total_cost")),
                    placed_at = reader.GetDateTime(reader.GetOrdinal("placed_at")),
                    fulfilled_at = reader.IsDBNull(reader.GetOrdinal("fulfilled_at")) ? null : reader.GetDateTime(reader.GetOrdinal("fulfilled_at")),
                    item_count = reader.GetInt32(reader.GetOrdinal("item_count")),
                    StatusValue = reader.GetByte(reader.GetOrdinal("StatusValue")),
                    StatusName = reader.GetString(reader.GetOrdinal("StatusName")),
                    payment_method = reader.IsDBNull(reader.GetOrdinal("payment_method")) ? null : reader.GetString(reader.GetOrdinal("payment_method")),
                    cancellation_requested = reader.GetBoolean(reader.GetOrdinal("cancellation_requested")),
                    cancellation_reason = reader.IsDBNull(reader.GetOrdinal("cancellation_reason")) ? null : reader.GetString(reader.GetOrdinal("cancellation_reason"))
                });
            }

            return orders;
        }

        // ============================================
        // 6. GET PENDING CANCELLATIONS
        // ============================================
        public async Task<List<PendingCancellationDTO>> GetPendingCancellationsAsync()
        {
            var cancellations = new List<PendingCancellationDTO>();

            using var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "sp_GetPendingCancellations";
            command.CommandType = CommandType.StoredProcedure;

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                cancellations.Add(new PendingCancellationDTO
                {
                    orders_id = reader.GetInt32(reader.GetOrdinal("orders_id")),
                    users_id = reader.GetString(reader.GetOrdinal("users_id")),
                    cancellation_reason = reader.IsDBNull(reader.GetOrdinal("cancellation_reason")) ? null : reader.GetString(reader.GetOrdinal("cancellation_reason")),
                    item_count = reader.GetInt32(reader.GetOrdinal("item_count")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    total_cost = reader.GetDecimal(reader.GetOrdinal("total_cost")),
                    placed_at = reader.GetDateTime(reader.GetOrdinal("placed_at"))
                });
            }

            return cancellations;
        }

        // ============================================
        // 7. REVIEW CANCELLATION
        // ============================================
        public async Task<string> ReviewCancellationAsync(int orderId, bool approve)
        {
            var resultMessage = new SqlParameter
            {
                ParameterName = "@ResultMessage",
                SqlDbType = SqlDbType.NVarChar,
                Size = 500,
                Direction = ParameterDirection.Output
            };

            await _db.Database.ExecuteSqlRawAsync(
                "EXEC sp_ReviewCancellation @OrderId, @Approve, @UserRole, @ResultMessage OUTPUT",
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@Approve", approve),
                new SqlParameter("@UserRole", "admin"),
                resultMessage
            );

            return resultMessage.Value?.ToString() ?? "Unknown error";
        }
    }
}