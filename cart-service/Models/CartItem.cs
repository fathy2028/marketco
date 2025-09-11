using System.Text.Json.Serialization;

namespace CartService.Models
{
    public class CartItem
    {
        public string CartItemId { get; set; } = Guid.NewGuid().ToString();
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryTime { get; set; }
        public CartTtlType TtlType { get; set; } = CartTtlType.Default;

        [JsonIgnore]
        public string RedisKey => $"cart:user:{UserId}:item:{CartItemId}";
    }

    public enum CartTtlType
    {
        Default,        // 24 hours
        OrderPlaced,    // 7 days
        PaymentCompleted // Indefinite
    }

    public class Cart
    {
        public long UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(i => i.Price * i.Quantity);
        public int TotalItems => Items.Sum(i => i.Quantity);
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public CartTtlType TtlType { get; set; } = CartTtlType.Default;
    }

    public class AddToCartRequest
    {
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Quantity { get; set; }
    }

    public class CartTtlUpdateRequest
    {
        public long UserId { get; set; }
        public CartTtlType TtlType { get; set; }
    }
}
