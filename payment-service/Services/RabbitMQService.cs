using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace PaymentService.Services
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
            // Declare payment exchange
            _channel.ExchangeDeclare(exchange: "payment.exchange", type: ExchangeType.Direct, durable: true);

            // Declare payment queues
            _channel.QueueDeclare(queue: "payment.events", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(queue: "payment.completed", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(queue: "payment.failed", durable: true, exclusive: false, autoDelete: false);

            // Bind queues to exchange
            _channel.QueueBind(queue: "payment.events", exchange: "payment.exchange", routingKey: "payment.events");
            _channel.QueueBind(queue: "payment.completed", exchange: "payment.exchange", routingKey: "payment.completed");
            _channel.QueueBind(queue: "payment.failed", exchange: "payment.exchange", routingKey: "payment.failed");

            // Declare order exchange (for publishing events to order service)
            _channel.ExchangeDeclare(exchange: "order.exchange", type: ExchangeType.Direct, durable: true);
        }

        public async Task PublishPaymentEventAsync(string eventType, object eventData)
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

                var routingKey = eventType switch
                {
                    "payment.completed" => "payment.completed",
                    "payment.failed" => "payment.failed",
                    _ => "payment.events"
                };

                _channel.BasicPublish(
                    exchange: "payment.exchange",
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published payment event: {EventType}", eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish payment event: {EventType}", eventType);
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
                    routingKey: "order.payment.status",
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published order event from payment: {EventType}", eventType);
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
