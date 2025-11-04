using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.Infrastructure;
using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found in appsettings.json");
}

Console.WriteLine($"[CONFIG] Connection String: {connectionString}");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "?? E-Commerce Shopping Cart API",
        Version = "v1.0",
        Description = @"
## Welcome to the E-Commerce Cart API! ??

A modern **REST API** for managing shopping carts, built with:
- ? **Domain-Driven Design (DDD)** principles
- ?? **Event-Driven Architecture** for real-time updates
- ?? **SQL Server** integration with full persistence
- ?? **Type-safe** operations with comprehensive validation

### Features
- ??? **Cart Management**: Create, view, and manage shopping carts
- ?? **Product Operations**: Add/remove products with stock validation
- ?? **Payment Processing**: Secure cart payment with transaction tracking
- ?? **Real-time Events**: Domain events for all cart operations
- ?? **Clean Architecture**: Separation of concerns with DDD patterns

### Quick Start
1. Use the **View Cart** endpoint to see a customer's current cart
2. **Add products** to the cart - stock is validated automatically
3. **Mark as paid** when checkout is complete

### Database
Connected to SQL Server with 10 sample products and 5 test customers ready to use!

---
*Built with ?? using .NET 9 and Entity Framework Core*
        ",
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

    // Add XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add tags with descriptions
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        if (api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerActionDescriptor)
        {
            return new[] { controllerActionDescriptor.ControllerName };
        }

        throw new InvalidOperationException("Unable to determine tag for endpoint.");
    });

    c.DocInclusionPredicate((name, api) => true);

    // Add examples
    c.EnableAnnotations();
});

// Add database services
builder.Services.AddECommerceDatabase(connectionString);

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

// Register CartApiService as scoped (will be initialized on first request)
builder.Services.AddScoped<CartApiService>();

var app = builder.Build();

// Initialize database
Console.WriteLine("[DATABASE] Initializing database...");
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        
        await services.InitializeDatabaseAsync();
        await services.SeedDatabaseAsync();
        Console.WriteLine("[DATABASE] Database initialized successfully!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[CRITICAL ERROR] Database initialization failed: {ex.Message}");
    Console.WriteLine($"[INFO] Please ensure SQL Server is running and accessible.");
    Console.WriteLine($"[DEBUG] Inner exception: {ex.InnerException?.Message}");
    throw;
}

// Setup event bus
SetupEventBusSubscriptions();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce Cart API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        
        // Custom styling
        c.InjectStylesheet("/swagger-custom.css");
        
        // Display request duration
        c.DisplayRequestDuration();
        
        // Enable deep linking
        c.EnableDeepLinking();
        
        // Enable filter
        c.EnableFilter();
        
        // Show extensions
        c.ShowExtensions();
        
        // Enable validator
        c.EnableValidator();
        
        // Default models expand depth
        c.DefaultModelsExpandDepth(2);
        
        // Default model expand depth
        c.DefaultModelExpandDepth(2);
        
        // Document title
        c.DocumentTitle = "?? E-Commerce Cart API - Interactive Documentation";
        
        // Custom CSS for additional tweaks
        c.InjectJavascript("/swagger-custom.js");
    });
}

// Serve static files for custom CSS/JS
app.UseStaticFiles();

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("\n=== E-Commerce Shopping Cart API ===");
Console.WriteLine("Event-Driven Architecture with DDD");
Console.WriteLine("Database-Integrated Workflow\n");
Console.WriteLine("API Endpoints:");
Console.WriteLine("  GET    /api/cart/view/{customerName}");
Console.WriteLine("  POST   /api/cart/add-product");
Console.WriteLine("  POST   /api/cart/mark-paid");
Console.WriteLine("  GET    /api/cart/active-carts");
Console.WriteLine("  GET    /api/cart/health");
Console.WriteLine("\nSwagger UI available at: http://localhost:5000 or https://localhost:5001");
Console.WriteLine("\nPress Ctrl+C to stop the server\n");

app.Run();

// Setup EventBus subscriptions for cross-context communication
static void SetupEventBusSubscriptions()
{
    Console.WriteLine("[EVENTBUS] Setting up event subscriptions...");

    EventBus.Subscribe<CartEvents.ProdusAdaugatInCosEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] Product added to cart: {evt.NumeProdus}, Stock decreased");
    });

    EventBus.Subscribe<CartEvents.ProdusStergeDinCosEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] Product removed from cart: {evt.NumeProdus}, Stock increased");
    });

    EventBus.Subscribe<CartEvents.CosGolitEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] Cart emptied, {evt.ProduseReturnate.Count} products returned to stock");
    });

    EventBus.Subscribe<CartEvents.CosPlatitEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] Cart paid by {evt.NumePersoana}, Total: {evt.TotalPlatit} lei");
    });

    EventBus.Subscribe<CartEvents.CosCreatEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] New cart created for {evt.NumePersoana}");
    });

    EventBus.Subscribe<InventoryEvents.ProdusEpuizatEvent>(evt =>
    {
        Console.WriteLine($"[WARNING] Product {evt.NumeProdus} is OUT OF STOCK!");
    });

    EventBus.Subscribe<InventoryEvents.ProdusDisponibilEvent>(evt =>
    {
        Console.WriteLine($"[INFO] Product {evt.NumeProdus} is now AVAILABLE (Stock: {evt.StocDisponibil})");
    });

    Console.WriteLine("[EVENTBUS] Event subscriptions configured!\n");
}
