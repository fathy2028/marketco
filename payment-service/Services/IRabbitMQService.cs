namespace PaymentService.Services
{
    public interface IRabbitMQService
    {
        Task PublishPaymentEventAsync(string eventType, object eventData);
        Task PublishOrderEventAsync(string eventType, object eventData);
    }
}


