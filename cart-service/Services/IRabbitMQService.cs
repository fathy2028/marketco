namespace CartService.Services
{
    public interface IRabbitMQService
    {
        Task PublishCartEventAsync(string eventType, object eventData);
        Task PublishOrderEventAsync(string eventType, object eventData);
    }
}
