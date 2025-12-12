using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.clase.Workflow.Orchestration;
using Lucrarea1PSSC.clase.Workflow;
using Lucrarea1PSSC.api.Services;
using Lucrarea1PSSC.api.Services.Delivery;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

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
var connectionString = builder.Configuration.GetConnectionString("ECommerceDB");
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("[STARTUP] Configuring database connection...");
    
    builder.Services.AddDbContext<ECommerceDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    builder.Services.AddScoped<UnitOfWork>();
    builder.Services.AddScoped<OrderWorkflowDatabaseService>();
    
    Console.WriteLine("[STARTUP] Database services configured");
}
else
{
    Console.WriteLine("[STARTUP] No database connection string found - running in memory mode");
}

// Configure Delivery API HttpClient with Polly Retry Policy
builder.Services.AddHttpClient<DeliveryApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "ECommerceCartAPI/1.0");
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

Console.WriteLine("[STARTUP] Delivery API client configured with retry policy (3 retries, exponential backoff)");

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
    return new CartApiService(dbService, deliveryClient, orchestrator);
});

Console.WriteLine("[STARTUP] Cart API Service registered");

var app = builder.Build();

// Demonstrate message queue patterns
Console.WriteLine("\n????????????????????????????????????????????????????????????????????????");
Console.WriteLine("  DEMONSTRATING MESSAGE QUEUE PATTERNS");
Console.WriteLine("????????????????????????????????????????????????????????????????????????\n");

await SimpleQueueExample.DemonstrateQueue();
await TopicExample.DemonstrateTopic();

Console.WriteLine("????????????????????????????????????????????????????????????????????????\n");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce Cart API v1");
        c.RoutePrefix = string.Empty;
        c.InjectStylesheet("/swagger-custom.css");
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
        c.DocumentTitle = "E-Commerce Cart API - Interactive Documentation";
        c.InjectJavascript("/swagger-custom.js");
    });
}

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

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
