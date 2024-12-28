using Microsoft.Extensions.Logging;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Services;

namespace YTM.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            ICartService cartService,
            IProductService productService,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
        }

        public async Task<Order> CreateOrderFromCartAsync(string userId, string shippingAddress, string paymentMethod)
        {
            try
            {
                _logger.LogInformation($"Creating order for user: {userId}");
                
                var cart = await _cartService.GetCartAsync(userId);
                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogWarning($"Cart is empty for user: {userId}");
                    throw new Exception("Sepetiniz boş. Sipariş oluşturulamadı!");
                }

                // Stok kontrolü
                foreach (var item in cart.Items)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product == null || product.Stock < item.Quantity)
                    {
                        _logger.LogWarning($"Insufficient stock for product: {item.ProductId}");
                        throw new Exception($"Yetersiz stok: {item.ProductName}");
                    }
                }

                var order = new Order
                {
                    UserId = userId,
                    Items = cart.Items.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Price = item.Price,
                        Quantity = item.Quantity
                    }).ToList(),
                    TotalAmount = cart.TotalAmount,
                    ShippingAddress = shippingAddress,
                    PaymentMethod = paymentMethod,
                    Status = "Pending",
                    OrderDate = DateTime.UtcNow
                };

                _logger.LogInformation($"Saving order to database for user: {userId}");
                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                // Stokları güncelle
                foreach (var item in cart.Items)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        await _productService.UpdateProductAsync(product.Id!, product);
                        _logger.LogInformation($"Updated stock for product: {product.Id}");
                    }
                }

                // Sepeti temizle
                await _cartService.ClearCartAsync(userId);
                _logger.LogInformation($"Cart cleared for user: {userId}");

                return createdOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating order: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _orderRepository.GetUserOrdersAsync(userId);
        }

        public async Task<Order?> GetOrderByIdAsync(string orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }

        public async Task UpdateOrderStatusAsync(string orderId, string status)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        }
    }
} 