using Microsoft.Extensions.Logging;
using YTM.Core.Entities;
using YTM.Core.Repositories;
using YTM.Core.Services;

namespace YTM.Service.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductService _productService;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IProductService productService,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productService = productService;
            _logger = logger;
        }

        public async Task<Cart?> GetCartAsync(string userId)
        {
            return await _cartRepository.GetCartByUserIdAsync(userId);
        }

        public async Task AddToCartAsync(string userId, string productId, int quantity)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    throw new Exception("Ürün bulunamadı");
                }

                if (product.Stock < quantity)
                {
                    throw new Exception("Yetersiz stok");
                }

                var cartItem = new CartItem
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Quantity = quantity,
                    Price = product.Price
                };

                await _cartRepository.AddItemToCartAsync(userId, cartItem);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding to cart: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveFromCartAsync(string userId, string productId)
        {
            await _cartRepository.RemoveItemFromCartAsync(userId, productId);
        }

        public async Task UpdateQuantityAsync(string userId, string productId, int quantity)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    throw new Exception("Ürün bulunamadı");
                }

                if (product.Stock < quantity)
                {
                    throw new Exception("Yetersiz stok");
                }

                await _cartRepository.UpdateItemQuantityAsync(userId, productId, quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating quantity: {ex.Message}");
                throw;
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            await _cartRepository.ClearCartAsync(userId);
        }
    }
} 