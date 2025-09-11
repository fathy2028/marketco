using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentDbContext _context;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(PaymentDbContext context, IRabbitMQService rabbitMQService, ILogger<PaymentService> logger)
        {
            _context = context;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // Create payment record
                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    UserId = request.UserId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentGateway = request.PaymentGateway ?? "Default",
                    Currency = request.Currency,
                    Notes = request.Notes,
                    Status = PaymentStatus.Processing,
                    PaymentDate = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Simulate payment processing
                var paymentResult = await SimulatePaymentProcessingAsync(payment);

                // Update payment status
                payment.Status = paymentResult.IsSuccess ? PaymentStatus.Completed : PaymentStatus.Failed;
                payment.ProcessedAt = DateTime.UtcNow;
                payment.TransactionId = paymentResult.TransactionId;
                payment.FailureReason = paymentResult.FailureReason;

                await _context.SaveChangesAsync();

                // Publish payment event
                await _rabbitMQService.PublishPaymentEventAsync(
                    paymentResult.IsSuccess ? "payment.completed" : "payment.failed",
                    payment);

                // If payment successful, publish order update event
                if (paymentResult.IsSuccess)
                {
                    await _rabbitMQService.PublishOrderEventAsync("order.payment.completed", new
                    {
                        OrderId = payment.OrderId,
                        PaymentId = payment.PaymentId,
                        Amount = payment.Amount,
                        UserId = payment.UserId
                    });
                }

                _logger.LogInformation("Payment {PaymentId} processed with status {Status}",
                    payment.PaymentId, payment.Status);

                return MapToResponse(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
                throw;
            }
        }

        public async Task<PaymentResponse?> GetPaymentAsync(long paymentId)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                return payment != null ? MapToResponse(payment) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
                return null;
            }
        }

        public async Task<List<PaymentResponse>> GetPaymentsByOrderAsync(long orderId)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.OrderId == orderId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return payments.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for order {OrderId}", orderId);
                return new List<PaymentResponse>();
            }
        }

        public async Task<List<PaymentResponse>> GetPaymentsByUserAsync(long userId)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return payments.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments for user {UserId}", userId);
                return new List<PaymentResponse>();
            }
        }

        public async Task<List<PaymentResponse>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.Status == status)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return payments.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments with status {Status}", status);
                return new List<PaymentResponse>();
            }
        }

        public async Task<PaymentResponse?> UpdatePaymentStatusAsync(long paymentId, PaymentStatus status, string? reason = null)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment == null)
                {
                    return null;
                }

                var oldStatus = payment.Status;
                payment.Status = status;

                if (status == PaymentStatus.Failed && !string.IsNullOrEmpty(reason))
                {
                    payment.FailureReason = reason;
                }

                if (status == PaymentStatus.Completed)
                {
                    payment.ProcessedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Publish status change event
                await _rabbitMQService.PublishPaymentEventAsync("payment.status.changed", new
                {
                    PaymentId = payment.PaymentId,
                    OrderId = payment.OrderId,
                    UserId = payment.UserId,
                    OldStatus = oldStatus.ToString(),
                    NewStatus = status.ToString(),
                    Reason = reason
                });

                _logger.LogInformation("Payment {PaymentId} status updated from {OldStatus} to {NewStatus}",
                    paymentId, oldStatus, status);

                return MapToResponse(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for payment {PaymentId}", paymentId);
                return null;
            }
        }

        public async Task<PaymentResponse?> RefundPaymentAsync(RefundRequest request)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(request.PaymentId);
                if (payment == null || payment.Status != PaymentStatus.Completed)
                {
                    return null;
                }

                var refundAmount = request.Amount ?? payment.Amount;
                if (refundAmount > payment.Amount)
                {
                    throw new InvalidOperationException("Refund amount cannot exceed payment amount");
                }

                // Simulate refund processing
                var refundResult = await SimulateRefundProcessingAsync(payment, refundAmount);

                if (refundResult.IsSuccess)
                {
                    payment.Status = PaymentStatus.Refunded;
                    payment.Notes = $"Refunded: {refundAmount:C}. Reason: {request.Reason}";
                    await _context.SaveChangesAsync();

                    // Publish refund event
                    await _rabbitMQService.PublishPaymentEventAsync("payment.refunded", new
                    {
                        PaymentId = payment.PaymentId,
                        OrderId = payment.OrderId,
                        UserId = payment.UserId,
                        RefundAmount = refundAmount,
                        Reason = request.Reason
                    });

                    _logger.LogInformation("Payment {PaymentId} refunded amount {Amount}",
                        payment.PaymentId, refundAmount);
                }

                return MapToResponse(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", request.PaymentId);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetPaymentStatisticsAsync()
        {
            try
            {
                var totalPayments = await _context.Payments.CountAsync();
                var totalAmount = await _context.Payments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .SumAsync(p => p.Amount);

                var statusCounts = await _context.Payments
                    .GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                var todayPayments = await _context.Payments
                    .Where(p => p.PaymentDate.Date == DateTime.Today)
                    .CountAsync();

                var todayRevenue = await _context.Payments
                    .Where(p => p.PaymentDate.Date == DateTime.Today && p.Status == PaymentStatus.Completed)
                    .SumAsync(p => p.Amount);

                return new Dictionary<string, object>
                {
                    { "totalPayments", totalPayments },
                    { "totalAmount", totalAmount },
                    { "todayPayments", todayPayments },
                    { "todayRevenue", todayRevenue },
                    { "statusBreakdown", statusCounts.ToDictionary(x => x.Status.ToString(), x => x.Count) }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment statistics");
                return new Dictionary<string, object>();
            }
        }

        public async Task<List<PaymentResponse>> GetPaymentHistoryAsync(long userId, int page = 0, int size = 10)
        {
            try
            {
                var payments = await _context.Payments
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.PaymentDate)
                    .Skip(page * size)
                    .Take(size)
                    .ToListAsync();

                return payments.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
                return new List<PaymentResponse>();
            }
        }

        private async Task<(bool IsSuccess, string? TransactionId, string? FailureReason)> SimulatePaymentProcessingAsync(Payment payment)
        {
            // Simulate processing delay
            await Task.Delay(1000);

            // Simulate different payment scenarios
            var random = new Random();
            var successRate = payment.PaymentMethod.ToLower() switch
            {
                "credit_card" => 0.95,
                "debit_card" => 0.90,
                "paypal" => 0.98,
                "bank_transfer" => 0.85,
                _ => 0.90
            };

            var isSuccess = random.NextDouble() < successRate;

            if (isSuccess)
            {
                return (true, $"TXN_{DateTime.UtcNow.Ticks}", null);
            }
            else
            {
                var failureReasons = new[]
                {
                    "Insufficient funds",
                    "Card declined",
                    "Payment gateway timeout",
                    "Invalid payment details",
                    "Fraud detection triggered"
                };

                return (false, null, failureReasons[random.Next(failureReasons.Length)]);
            }
        }

        private async Task<(bool IsSuccess, string? FailureReason)> SimulateRefundProcessingAsync(Payment payment, decimal amount)
        {
            // Simulate refund processing
            await Task.Delay(500);

            // Simulate 98% success rate for refunds
            var random = new Random();
            var isSuccess = random.NextDouble() < 0.98;

            return isSuccess
                ? (true, null)
                : (false, "Refund processing failed - please contact payment gateway");
        }

        private static PaymentResponse MapToResponse(Payment payment)
        {
            return new PaymentResponse
            {
                PaymentId = payment.PaymentId,
                OrderId = payment.OrderId,
                UserId = payment.UserId,
                Amount = payment.Amount,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                TransactionId = payment.TransactionId,
                PaymentGateway = payment.PaymentGateway,
                FailureReason = payment.FailureReason,
                ProcessedAt = payment.ProcessedAt,
                Currency = payment.Currency,
                Notes = payment.Notes
            };
        }
    }
}


