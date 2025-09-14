using CartService.Models;
using CartService.Services;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add configuration

// Add services to the container
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "E-commerce Cart Service API",
        Version = "v1.0.0",
        Description = "Cart Management Service with TTL Logic for E-commerce Platform",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "E-commerce Team",
            Email = "support@ecommerce.com",
            Url = new Uri("https://ecommerce.com")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = provider.GetService<IConfiguration>();
    var connectionString = configuration?.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

// Service discovery will be handled manually

// Register custom services
builder.Services.AddScoped<ICartService, CartService.Services.CartService>();
builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure port
var port = builder.Configuration["Eureka:Instance:Port"] ?? "5001";
app.Urls.Add($"http://0.0.0.0:{port}");

// Configure the HTTP request pipeline
app.UseRouting();
app.UseCors("AllowAll");

// Enable Swagger for all environments to allow API testing
app.UseSwagger(c =>
{
    c.SerializeAsV2 = false;
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-commerce Cart Service API v1");
    c.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
    c.EnableDeepLinking();
    c.DisplayOperationId();
});

// Add Prometheus metrics
app.UseMetricServer();
app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();
app.MapMetrics();

// Register with Eureka
var eurekaUrl = "http://discovery-service:8761/eureka/apps/cart-service";
var xmlData = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<application>
  <name>CART-SERVICE</name>
  <instance>
    <instanceId>cart-service:5001</instanceId>
    <hostName>cart-service</hostName>
    <app>CART-SERVICE</app>
    <ipAddr>cart-service</ipAddr>
    <status>UP</status>
    <overriddenstatus>UNKNOWN</overriddenstatus>
    <port enabled=""true"">5001</port>
    <securePort enabled=""false"">443</securePort>
    <countryId>1</countryId>
    <dataCenterInfo class=""com.netflix.appinfo.InstanceInfo$DefaultDataCenterInfo"">
      <name>MyOwn</name>
    </dataCenterInfo>
    <leaseInfo>
      <renewalIntervalInSecs>30</renewalIntervalInSecs>
      <durationInSecs>90</durationInSecs>
    </leaseInfo>
    <homePageUrl>http://cart-service:5001/</homePageUrl>
    <statusPageUrl>http://cart-service:5001/info</statusPageUrl>
    <healthCheckUrl>http://cart-service:5001/health</healthCheckUrl>
    <vipAddress>cart-service</vipAddress>
    <secureVipAddress>cart-service</secureVipAddress>
    <isCoordinatingDiscoveryServer>false</isCoordinatingDiscoveryServer>
    <actionType>ADDED</actionType>
  </instance>
</application>";

// Register with Eureka in background
_ = Task.Run(async () =>
{
    await Task.Delay(5000); // Wait 5 seconds for service to start
    using var httpClient = new HttpClient();
    try
    {
        var content = new StringContent(xmlData, System.Text.Encoding.UTF8, "application/xml");
        await httpClient.PostAsync(eurekaUrl, content);
        Console.WriteLine("Registered with Eureka");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to register with Eureka: {ex.Message}");
    }
});

app.Run();
