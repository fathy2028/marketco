using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;
using PaymentService.Services;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var payment = await _paymentService.ProcessPaymentAsync(request);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponse>> GetPayment(long id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentAsync(id);
                if (payment == null)
                {
                    return NotFound();
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<List<PaymentResponse>>> GetPaymentsByOrder(long orderId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByOrderAsync(orderId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for order {OrderId}", orderId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<PaymentResponse>>> GetPaymentsByUser(long userId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByUserAsync(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<List<PaymentResponse>>> GetPaymentsByStatus(PaymentStatus status)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByStatusAsync(status);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments with status {Status}", status);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<PaymentResponse>> UpdatePaymentStatus(long id, [FromQuery] PaymentStatus status, [FromQuery] string? reason = null)
        {
            try
            {
                var payment = await _paymentService.UpdatePaymentStatusAsync(id, status, reason);
                if (payment == null)
                {
                    return NotFound();
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for payment {PaymentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("refund")]
        public async Task<ActionResult<PaymentResponse>> RefundPayment([FromBody] RefundRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var payment = await _paymentService.RefundPaymentAsync(request);
                if (payment == null)
                {
                    return NotFound("Payment not found or cannot be refunded");
                }

                return Ok(payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", request.PaymentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<Dictionary<string, object>>> GetPaymentStatistics()
        {
            try
            {
                var statistics = await _paymentService.GetPaymentStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<ActionResult<List<PaymentResponse>>> GetPaymentHistory(long userId, [FromQuery] int page = 0, [FromQuery] int size = 10)
        {
            try
            {
                if (size > 100)
                {
                    size = 100; // Limit page size
                }

                var payments = await _paymentService.GetPaymentHistoryAsync(userId, page, size);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}


