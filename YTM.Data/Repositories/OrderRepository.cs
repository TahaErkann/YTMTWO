using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Settings;

namespace YTM.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _orders;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(IOptions<DatabaseSettings> settings, ILogger<OrderRepository> logger)
        {
            _logger = logger;
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _orders = database.GetCollection<Order>(settings.Value.OrdersCollectionName);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                await _orders.InsertOneAsync(order);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _orders.Find(o => o.UserId == userId)
                              .SortByDescending(o => o.OrderDate)
                              .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            return await _orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();
        }

        public async Task UpdateOrderStatusAsync(string orderId, string status)
        {
            var update = Builders<Order>.Update.Set(o => o.Status, status);
            await _orders.UpdateOneAsync(o => o.Id == orderId, update);
        }
    }
} 