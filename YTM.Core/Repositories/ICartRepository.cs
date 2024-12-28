using YTM.Core.Entities;

namespace YTM.Core.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(string userId);
        Task CreateCartAsync(Cart cart);
        Task UpdateCartAsync(string userId, Cart cart);
        Task DeleteCartAsync(string userId);
        Task AddItemToCartAsync(string userId, CartItem item);
        Task RemoveItemFromCartAsync(string userId, string productId);
        Task UpdateItemQuantityAsync(string userId, string productId, int quantity);
        Task ClearCartAsync(string userId);
    }
} 