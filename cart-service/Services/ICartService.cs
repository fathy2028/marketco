using CartService.Models;

namespace CartService.Services
{
    public interface ICartService
    {
        Task<Cart> GetCartAsync(long userId);
        Task<CartItem> AddToCartAsync(AddToCartRequest request);
        Task<CartItem?> UpdateCartItemAsync(long userId, string cartItemId, UpdateCartItemRequest request);
        Task<bool> RemoveCartItemAsync(long userId, string cartItemId);
        Task<bool> ClearCartAsync(long userId);
        Task<bool> UpdateCartTtlAsync(long userId, CartTtlType ttlType);
        Task<List<CartItem>> GetExpiredItemsAsync();
        Task<bool> RemoveExpiredItemsAsync();
        Task<Dictionary<string, object>> GetCartStatisticsAsync();
    }
}
