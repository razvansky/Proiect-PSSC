using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.clase.Workflow.Orchestration;
using Lucrarea1PSSC.clase.Workflow;
using Lucrarea1PSSC.api.Services;
using Lucrarea1PSSC.api.Services.Delivery;
using Lucrarea1PSSC.clase.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // Azure App Service sets PORT (commonly 8080). Fall back to 8080.
    var portValue = Environment.GetEnvironmentVariable("PORT")
                   ?? Environment.GetEnvironmentVariable("WEBSITES_PORT")
                   ?? "8080";

    if (!int.TryParse(portValue, out var port))
        port = 8080;

    options.ListenAnyIP(port);
});

// Load configuration from resources folder
builder.Configuration.AddJsonFile("resources/appsettings.json", optional: true, reloadOnChange: true);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "E-Commerce Shopping Cart API",
        Version = "v1.0",
        Description = "Modern REST API with Event-Driven Architecture, Message Queues, and Workflows including Order Pickup (Preluare Comanda)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "E-Commerce Development Team",
            Email = "support@ecommerce-api.com",
            Url = new Uri("https://github.com/razvansky/Lucrarea1PSSC")
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.EnableAnnotations();
});

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString(ECommerceDbContext.ConnectionStringName);
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("[STARTUP] Configuring database connection...");

    builder.Services.AddDbContext<ECommerceDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

    builder.Services.AddScoped<UnitOfWork>();
    builder.Services.AddScoped<OrderWorkflowDatabaseService>();

    Console.WriteLine("[STARTUP] Database services configured");
}
else
{
    Console.WriteLine("[STARTUP] No database connection string found - running in memory mode");
}

// Configure Delivery API HttpClient with Polly Retry Policy
var deliveryBaseUrl = builder.Configuration["DeliveryApi:BaseUrl"] ?? "http://localhost:5000";

builder.Services.AddHttpClient<DeliveryApiClient>(client =>
{
    client.BaseAddress = new Uri(deliveryBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "ECommerceCartAPI/1.0");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

Console.WriteLine("[STARTUP] Delivery API client configured with retry policy (3 retries, exponential backoff)");

// IMessageBus for future inter-context communication (Billing/Shipping/Order)
builder.Services.AddSingleton<IMessageBus, InMemoryMessageBus>();

// Register Order Processing Orchestrator as Singleton (message bus should be singleton)
builder.Services.AddSingleton<OrderProcessingOrchestrator>(serviceProvider =>
{
    var deliveryClient = serviceProvider.GetService<DeliveryApiClient>();
    return new OrderProcessingOrchestrator(deliveryClient);
});

Console.WriteLine("[STARTUP] Order Processing Orchestrator registered");

// Register PreluareComandaWorkflow as Singleton (maintains order repository state)
builder.Services.AddSingleton<PreluareComandaWorkflow>(serviceProvider =>
{
    var dbService = serviceProvider.GetService<OrderWorkflowDatabaseService>();
    return new PreluareComandaWorkflow(dbService);
});

Console.WriteLine("[STARTUP] Preluare Comanda Workflow registered");

// Register Cart API Service (works with or without database)
builder.Services.AddScoped<CartApiService>(serviceProvider =>
{
    var dbService = serviceProvider.GetService<OrderWorkflowDatabaseService>();
    var deliveryClient = serviceProvider.GetService<DeliveryApiClient>();
    var orchestrator = serviceProvider.GetService<OrderProcessingOrchestrator>();
    var messageBus = serviceProvider.GetService<IMessageBus>();
    return new CartApiService(dbService, deliveryClient, orchestrator, messageBus);
});

Console.WriteLine("[STARTUP] Cart API Service registered");

var app = builder.Build();

// Initialize & seed database (safe; seeding skips if data exists)
if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        Console.WriteLine("[STARTUP] Initializing database...");
        await DatabaseConfiguration.InitializeDatabaseAsync(app.Services);
        await DatabaseConfiguration.SeedDatabaseAsync(app.Services);
        Console.WriteLine("[STARTUP] Database initialization complete");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[STARTUP][DB ERROR] Database init failed: {ex.Message}");
    }
}

// Demonstrate message queue patterns
Console.WriteLine("\n????????????????????????????????????????????????????????????????????????");
Console.WriteLine("  DEMONSTRATING MESSAGE QUEUE PATTERNS");
Console.WriteLine("????????????????????????????????????????????????????????????????????????\n");

await SimpleQueueExample.DemonstrateQueue();
await TopicExample.DemonstrateTopic();

Console.WriteLine("????????????????????????????????????????????????????????????????????????\n");

// Configure the HTTP request pipeline
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");

if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce Cart API v1");
        c.RoutePrefix = string.Empty; // Swagger at /
        c.InjectStylesheet("/swagger-custom.css");
        c.InjectJavascript("/swagger-custom.js");
    });
}

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok("OK"));

Console.WriteLine("[STARTUP] E-Commerce Cart API is starting...");
Console.WriteLine("[STARTUP] Swagger UI available at: http://localhost:5000");
Console.WriteLine("[STARTUP] Event-Driven Architecture with Message Queues enabled");
Console.WriteLine("[STARTUP] Preluare Comanda Workflow endpoints available at: /api/preluarecomanda");

app.Run();

// Polly Retry Policy: 3 retries with exponential backoff
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => (int)msg.StatusCode == 500)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"[POLLY RETRY] Attempt {retryCount} after {timespan.TotalSeconds}s delay");
            });
}

// Circuit Breaker Policy: Opens after 5 consecutive failures
static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (outcome, duration) =>
            {
                Console.WriteLine($"[POLLY CIRCUIT BREAKER] Circuit opened for {duration.TotalSeconds}s");
            },
            onReset: () =>
            {
                Console.WriteLine("[POLLY CIRCUIT BREAKER] Circuit reset - service is healthy again");
            });
}
