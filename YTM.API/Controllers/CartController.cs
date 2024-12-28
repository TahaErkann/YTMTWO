using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YTM.Core.Services;

namespace YTM.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirst("UserId")?.Value ?? 
                throw new InvalidOperationException("User ID not found in token");
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cart: {ex.Message}");
                return StatusCode(500, new { message = "Sepet bilgileri alınırken bir hata oluştu." });
            }
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ProductId) || request.ProductId.Length != 24)
                {
                    return BadRequest(new { message = "Geçersiz ürün ID'si" });
                }

                var userId = GetUserId();
                await _cartService.AddToCartAsync(userId, request.ProductId, request.Quantity);
                return Ok(new { message = "Ürün sepete eklendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to cart: {ex.Message}");
                return StatusCode(500, new { message = "Ürün sepete eklenirken bir hata oluştu." });
            }
        }

        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateQuantity(string productId, [FromBody] UpdateQuantityRequest request)
        {
            try
            {
                var userId = GetUserId();
                await _cartService.UpdateQuantityAsync(userId, productId, request.Quantity);
                return Ok(new { message = "Ürün miktarı güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating quantity: {ex.Message}");
                return StatusCode(500, new { message = "Ürün miktarı güncellenirken bir hata oluştu." });
            }
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            try
            {
                var userId = GetUserId();
                await _cartService.RemoveFromCartAsync(userId, productId);
                return Ok(new { message = "Ürün sepetten kaldırıldı." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing from cart: {ex.Message}");
                return StatusCode(500, new { message = "Ürün sepetten kaldırılırken bir hata oluştu." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                await _cartService.ClearCartAsync(userId);
                return Ok(new { message = "Sepet temizlendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error clearing cart: {ex.Message}");
                return StatusCode(500, new { message = "Sepet temizlenirken bir hata oluştu." });
            }
        }
    }

    public class AddToCartRequest
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class UpdateQuantityRequest
    {
        public int Quantity { get; set; }
    }
} 