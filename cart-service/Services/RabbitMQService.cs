using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace CartService.Services
{
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest"
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare exchanges and queues
                SetupRabbitMQ();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
        }

        private void SetupRabbitMQ()
        {
            // Declare cart exchange
            _channel.ExchangeDeclare(exchange: "cart.exchange", type: ExchangeType.Direct, durable: true);

            // Declare cart queues
            _channel.QueueDeclare(queue: "cart.events", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(queue: "cart.ttl.updated", durable: true, exclusive: false, autoDelete: false);

            // Bind queues to exchange
            _channel.QueueBind(queue: "cart.events", exchange: "cart.exchange", routingKey: "cart.events");
            _channel.QueueBind(queue: "cart.ttl.updated", exchange: "cart.exchange", routingKey: "cart.ttl.updated");

            // Declare order exchange (for publishing events to order service)
            _channel.ExchangeDeclare(exchange: "order.exchange", type: ExchangeType.Direct, durable: true);
        }

        public async Task PublishCartEventAsync(string eventType, object eventData)
        {
            try
            {
                var message = new
                {
                    EventType = eventType,
                    Timestamp = DateTime.UtcNow,
                    Data = eventData
                };

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                _channel.BasicPublish(
                    exchange: "cart.exchange",
                    routingKey: "cart.events",
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published cart event: {EventType}", eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish cart event: {EventType}", eventType);
            }
        }

        public async Task PublishOrderEventAsync(string eventType, object eventData)
        {
            try
            {
                var message = new
                {
                    EventType = eventType,
                    Timestamp = DateTime.UtcNow,
                    Data = eventData
                };

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                _channel.BasicPublish(
                    exchange: "order.exchange",
                    routingKey: "order.from.cart",
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published order event from cart: {EventType}", eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish order event: {EventType}", eventType);
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
