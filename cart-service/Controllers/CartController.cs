using CartService.Models;
using CartService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CartService.Controllers
{
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

        [HttpGet("{userId}")]
        public async Task<ActionResult<Cart>> GetCart(long userId)
        {
            try
            {
                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("add")]
        public async Task<ActionResult<CartItem>> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than 0");
                }

                if (request.Price < 0)
                {
                    return BadRequest("Price cannot be negative");
                }

                var cartItem = await _cartService.AddToCartAsync(request);
                return CreatedAtAction(nameof(GetCart), new { userId = request.UserId }, cartItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}", request.UserId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{userId}/items/{cartItemId}")]
        public async Task<ActionResult<CartItem>> UpdateCartItem(long userId, string cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than 0");
                }

                var cartItem = await _cartService.UpdateCartItemAsync(userId, cartItemId, request);
                if (cartItem == null)
                {
                    return NotFound("Cart item not found");
                }

                return Ok(cartItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {CartItemId} for user {UserId}", cartItemId, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{userId}/items/{cartItemId}")]
        public async Task<ActionResult> RemoveCartItem(long userId, string cartItemId)
        {
            try
            {
                var success = await _cartService.RemoveCartItemAsync(userId, cartItemId);
                if (!success)
                {
                    return NotFound("Cart item not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {CartItemId} for user {UserId}", cartItemId, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{userId}")]
        public async Task<ActionResult> ClearCart(long userId)
        {
            try
            {
                var success = await _cartService.ClearCartAsync(userId);
                if (!success)
                {
                    return NotFound("Cart not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{userId}/ttl")]
        public async Task<ActionResult> UpdateCartTtl(long userId, [FromBody] CartTtlUpdateRequest request)
        {
            try
            {
                var success = await _cartService.UpdateCartTtlAsync(userId, request.TtlType);
                if (!success)
                {
                    return NotFound("Cart not found or empty");
                }

                return Ok(new { Message = $"Cart TTL updated to {request.TtlType}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart TTL for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("expired")]
        public async Task<ActionResult<List<CartItem>>> GetExpiredItems()
        {
            try
            {
                var expiredItems = await _cartService.GetExpiredItemsAsync();
                return Ok(expiredItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expired cart items");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("expired")]
        public async Task<ActionResult> RemoveExpiredItems()
        {
            try
            {
                var success = await _cartService.RemoveExpiredItemsAsync();
                return Ok(new { Message = "Expired items removed", Success = success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing expired cart items");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<Dictionary<string, object>>> GetCartStatistics()
        {
            try
            {
                var statistics = await _cartService.GetCartStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{userId}/checkout")]
        public async Task<ActionResult> CheckoutCart(long userId)
        {
            try
            {
                var cart = await _cartService.GetCartAsync(userId);
                if (!cart.Items.Any())
                {
                    return BadRequest("Cart is empty");
                }

                // Update cart TTL to OrderPlaced (7 days)
                await _cartService.UpdateCartTtlAsync(userId, CartTtlType.OrderPlaced);

                // Here you would typically create an order in the Order Service
                // For now, we'll just return success
                return Ok(new
                {
                    Message = "Cart ready for checkout",
                    CartTotal = cart.TotalAmount,
                    ItemCount = cart.TotalItems,
                    TtlUpdated = "OrderPlaced (7 days)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cart checkout for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}


