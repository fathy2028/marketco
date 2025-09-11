using PaymentService.Models;

namespace PaymentService.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResponse?> GetPaymentAsync(long paymentId);
        Task<List<PaymentResponse>> GetPaymentsByOrderAsync(long orderId);
        Task<List<PaymentResponse>> GetPaymentsByUserAsync(long userId);
        Task<List<PaymentResponse>> GetPaymentsByStatusAsync(PaymentStatus status);
        Task<PaymentResponse?> UpdatePaymentStatusAsync(long paymentId, PaymentStatus status, string? reason = null);
        Task<PaymentResponse?> RefundPaymentAsync(RefundRequest request);
        Task<Dictionary<string, object>> GetPaymentStatisticsAsync();
        Task<List<PaymentResponse>> GetPaymentHistoryAsync(long userId, int page = 0, int size = 10);
    }
}
