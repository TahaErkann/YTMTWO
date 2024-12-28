using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YTM.Core.Services;

namespace YTM.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                _logger.LogInformation("CreateOrder endpoint called with data: {@request}", request);

                if (string.IsNullOrEmpty(request.ShippingAddress) || string.IsNullOrEmpty(request.PaymentMethod))
                {
                    _logger.LogWarning("Invalid request data: Address or payment method is empty");
                    return BadRequest(new { message = "Geçersiz sipariş bilgileri" });
                }

                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in token");
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı" });
                }

                _logger.LogInformation($"Attempting to create order for user: {userId}");

                var order = await _orderService.CreateOrderFromCartAsync(
                    userId,
                    request.ShippingAddress,
                    request.PaymentMethod);

                _logger.LogInformation($"Order created successfully. Order ID: {order.Id}");

                return Ok(new
                {
                    success = true,
                    message = "Sipariş başarıyla oluşturuldu",
                    orderId = order.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateOrder: {ex.Message}\nStack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            try
            {
                var userId = GetUserId();
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user orders: {ex.Message}");
                return StatusCode(500, new { message = "Siparişler alınırken bir hata oluştu" });
            }
        }

        private string GetUserId()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User ID not found in token");
            }
            return userId;
        }
    }

    public class CreateOrderRequest
    {
        public string ShippingAddress { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }
} 