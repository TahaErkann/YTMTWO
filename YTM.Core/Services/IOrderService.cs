using YTM.Core.Entities;

namespace YTM.Core.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderFromCartAsync(string userId, string shippingAddress, string paymentMethod);
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order?> GetOrderByIdAsync(string orderId);
        Task UpdateOrderStatusAsync(string orderId, string status);
    }
} 