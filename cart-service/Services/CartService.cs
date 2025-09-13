using CartService.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Text.Json;

namespace CartService.Services
{
    public class CartService : ICartService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<CartService> _logger;

        // TTL Constants
        private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(24);
        private static readonly TimeSpan OrderPlacedTtl = TimeSpan.FromDays(7);
        private static readonly TimeSpan? PaymentCompletedTtl = null; // Indefinite

        public CartService(IConnectionMultiplexer redis, IRabbitMQService rabbitMQService, ILogger<CartService> logger)
        {
            _redis = redis;
            _database = redis.GetDatabase();
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        public async Task<Cart> GetCartAsync(long userId)
        {
            try
            {
                var cartKey = $"cart:user:{userId}";
                var itemKeys = await _database.SetMembersAsync(cartKey);

                var cart = new Cart { UserId = userId };

                if (itemKeys.Length == 0)
                {
                    return cart;
                }

                var tasks = itemKeys.Select(async key =>
                {
                    var itemJson = await _database.StringGetAsync(key.ToString());
                    if (itemJson.HasValue)
                    {
                        return JsonConvert.DeserializeObject<CartItem>(itemJson);
                    }
                    return null;
                });

                var items = await Task.WhenAll(tasks);
                cart.Items = items.Where(item => item != null && item.ExpiryTime > DateTime.UtcNow).ToList()!;

                // Update cart TTL type based on items
                if (cart.Items.Any())
                {
                    cart.TtlType = cart.Items.Max(i => i.TtlType);
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {UserId}", userId);
                return new Cart { UserId = userId };
            }
        }

        public async Task<CartItem> AddToCartAsync(AddToCartRequest request)
        {
            try
            {
                var cartKey = $"cart:user:{request.UserId}";

                // Check if item already exists
                var existingItems = await GetCartAsync(request.UserId);
                var existingItem = existingItems.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

                if (existingItem != null)
                {
                    // Update quantity of existing item
                    existingItem.Quantity += request.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                    existingItem.ExpiryTime = CalculateExpiryTime(existingItem.TtlType);

                    var itemJson = JsonConvert.SerializeObject(existingItem);
                    await _database.StringSetAsync(existingItem.RedisKey, itemJson, GetTtlForType(existingItem.TtlType));

                    _logger.LogInformation("Updated cart item {CartItemId} for user {UserId}", existingItem.CartItemId, request.UserId);
                    return existingItem;
                }

                // Create new cart item
                var cartItem = new CartItem
                {
                    UserId = request.UserId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    ProductName = request.ProductName,
                    ProductDescription = request.ProductDescription,
                    ImageUrl = request.ImageUrl,
                    ExpiryTime = CalculateExpiryTime(CartTtlType.Default),
                    TtlType = CartTtlType.Default
                };

                // Store cart item
                var cartItemJson = JsonConvert.SerializeObject(cartItem);
                await _database.StringSetAsync(cartItem.RedisKey, cartItemJson, DefaultTtl);

                // Add to user's cart set
                await _database.SetAddAsync(cartKey, cartItem.RedisKey);
                await _database.KeyExpireAsync(cartKey, DefaultTtl);

                // Publish cart event
                await _rabbitMQService.PublishCartEventAsync("cart.item.added", cartItem);

                _logger.LogInformation("Added item {CartItemId} to cart for user {UserId}", cartItem.CartItemId, request.UserId);
                return cartItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}", request.UserId);
                throw;
            }
        }

        public async Task<CartItem?> UpdateCartItemAsync(long userId, string cartItemId, UpdateCartItemRequest request)
        {
            try
            {
                var itemKey = $"cart:user:{userId}:item:{cartItemId}";
                var itemJson = await _database.StringGetAsync(itemKey);

                if (!itemJson.HasValue)
                {
                    return null;
                }

                var cartItem = JsonConvert.DeserializeObject<CartItem>(itemJson);
                if (cartItem == null)
                {
                    return null;
                }

                cartItem.Quantity = request.Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
                cartItem.ExpiryTime = CalculateExpiryTime(cartItem.TtlType);

                var updatedJson = JsonConvert.SerializeObject(cartItem);
                await _database.StringSetAsync(itemKey, updatedJson, GetTtlForType(cartItem.TtlType));

                // Publish cart event
                await _rabbitMQService.PublishCartEventAsync("cart.item.updated", cartItem);

                _logger.LogInformation("Updated cart item {CartItemId} for user {UserId}", cartItemId, userId);
                return cartItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartItemId} for user {UserId}", cartItemId, userId);
                return null;
            }
        }

        public async Task<bool> RemoveCartItemAsync(long userId, string cartItemId)
        {
            try
            {
                var cartKey = $"cart:user:{userId}";
                var itemKey = $"cart:user:{userId}:item:{cartItemId}";

                // Get item before deletion for event publishing
                var itemJson = await _database.StringGetAsync(itemKey);
                CartItem? cartItem = null;
                if (itemJson.HasValue)
                {
                    cartItem = JsonConvert.DeserializeObject<CartItem>(itemJson);
                }

                // Remove from set and delete item
                await _database.SetRemoveAsync(cartKey, itemKey);
                var deleted = await _database.KeyDeleteAsync(itemKey);

                if (deleted && cartItem != null)
                {
                    // Publish cart event
                    await _rabbitMQService.PublishCartEventAsync("cart.item.removed", cartItem);
                    _logger.LogInformation("Removed cart item {CartItemId} for user {UserId}", cartItemId, userId);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartItemId} for user {UserId}", cartItemId, userId);
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(long userId)
        {
            try
            {
                var cartKey = $"cart:user:{userId}";
                var itemKeys = await _database.SetMembersAsync(cartKey);

                if (itemKeys.Length == 0)
                {
                    return true;
                }

                // Delete all cart items
                var deleteKeys = itemKeys.Select(key => (RedisKey)key.ToString()).ToArray();
                await _database.KeyDeleteAsync(deleteKeys);

                // Delete cart set
                await _database.KeyDeleteAsync(cartKey);

                // Publish cart event
                await _rabbitMQService.PublishCartEventAsync("cart.cleared", new { UserId = userId });

                _logger.LogInformation("Cleared cart for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateCartTtlAsync(long userId, CartTtlType ttlType)
        {
            try
            {
                var cart = await GetCartAsync(userId);
                if (!cart.Items.Any())
                {
                    return false;
                }

                var cartKey = $"cart:user:{userId}";
                var newTtl = GetTtlForType(ttlType);
                var newExpiryTime = CalculateExpiryTime(ttlType);

                // Update all cart items
                foreach (var item in cart.Items)
                {
                    item.TtlType = ttlType;
                    item.ExpiryTime = newExpiryTime;
                    item.UpdatedAt = DateTime.UtcNow;

                    var itemJson = JsonConvert.SerializeObject(item);
                    await _database.StringSetAsync(item.RedisKey, itemJson, newTtl);
                }

                // Update cart set TTL
                if (newTtl.HasValue)
                {
                    await _database.KeyExpireAsync(cartKey, newTtl);
                }
                else
                {
                    await _database.KeyPersistAsync(cartKey); // Remove expiration
                }

                // Publish TTL update event
                await _rabbitMQService.PublishCartEventAsync("cart.ttl.updated", new { UserId = userId, TtlType = ttlType });

                _logger.LogInformation("Updated cart TTL to {TtlType} for user {UserId}", ttlType, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart TTL for user {UserId}", userId);
                return false;
            }
        }

        public async Task<List<CartItem>> GetExpiredItemsAsync()
        {
            try
            {
                var expiredItems = new List<CartItem>();
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: "cart:user:*:item:*");

                foreach (var key in keys)
                {
                    var itemJson = await _database.StringGetAsync(key);
                    if (itemJson.HasValue)
                    {
                        var item = JsonConvert.DeserializeObject<CartItem>(itemJson);
                        if (item != null && item.ExpiryTime <= DateTime.UtcNow)
                        {
                            expiredItems.Add(item);
                        }
                    }
                }

                return expiredItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired cart items");
                return new List<CartItem>();
            }
        }

        public async Task<bool> RemoveExpiredItemsAsync()
        {
            try
            {
                var expiredItems = await GetExpiredItemsAsync();
                if (!expiredItems.Any())
                {
                    return true;
                }

                foreach (var item in expiredItems)
                {
                    await RemoveCartItemAsync(item.UserId, item.CartItemId);
                }

                _logger.LogInformation("Removed {Count} expired cart items", expiredItems.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing expired cart items");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetCartStatisticsAsync()
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var cartKeys = server.Keys(pattern: "cart:user:*");
                var itemKeys = server.Keys(pattern: "cart:user:*:item:*");

                var totalCarts = cartKeys.Count();
                var totalItems = itemKeys.Count();

                var ttlCounts = new Dictionary<CartTtlType, int>
                {
                    { CartTtlType.Default, 0 },
                    { CartTtlType.OrderPlaced, 0 },
                    { CartTtlType.PaymentCompleted, 0 }
                };

                foreach (var key in itemKeys)
                {
                    var itemJson = await _database.StringGetAsync(key);
                    if (itemJson.HasValue)
                    {
                        var item = JsonConvert.DeserializeObject<CartItem>(itemJson);
                        if (item != null)
                        {
                            ttlCounts[item.TtlType]++;
                        }
                    }
                }

                return new Dictionary<string, object>
                {
                    { "totalCarts", totalCarts },
                    { "totalItems", totalItems },
                    { "defaultTtlItems", ttlCounts[CartTtlType.Default] },
                    { "orderPlacedTtlItems", ttlCounts[CartTtlType.OrderPlaced] },
                    { "paymentCompletedTtlItems", ttlCounts[CartTtlType.PaymentCompleted] }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart statistics");
                return new Dictionary<string, object>();
            }
        }

        private static DateTime CalculateExpiryTime(CartTtlType ttlType)
        {
            return ttlType switch
            {
                CartTtlType.Default => DateTime.UtcNow.Add(DefaultTtl),
                CartTtlType.OrderPlaced => DateTime.UtcNow.Add(OrderPlacedTtl),
                CartTtlType.PaymentCompleted => DateTime.MaxValue, // Never expires
                _ => DateTime.UtcNow.Add(DefaultTtl)
            };
        }

        private static TimeSpan? GetTtlForType(CartTtlType ttlType)
        {
            return ttlType switch
            {
                CartTtlType.Default => DefaultTtl,
                CartTtlType.OrderPlaced => OrderPlacedTtl,
                CartTtlType.PaymentCompleted => null, // No expiration
                _ => DefaultTtl
            };
        }
    }
}


