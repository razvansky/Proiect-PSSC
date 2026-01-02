using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.clase.Workflow.Orchestration;
using Lucrarea1PSSC.api.Services.Delivery;
using Lucrarea1PSSC.api.DTOs;
using Lucrarea1PSSC.clase.Infrastructure.Messaging;

namespace Lucrarea1PSSC.api.Services
{
    public class CartApiService
    {
        private readonly OrderWorkflowDatabaseService? _dbService;
        private readonly DeliveryApiClient? _deliveryClient;
        private readonly OrderProcessingOrchestrator? _orchestrator;
        private readonly IMessageBus? _messageBus;
        private static List<Produs>? _produse;
        private static List<Persoana>? _persoane;
        private static readonly Dictionary<string, CosDeCumparaturi> _activeCarts = new();
        private static bool _initialized = false;
        private static readonly SemaphoreSlim _initLock = new(1, 1);

        public CartApiService(
            OrderWorkflowDatabaseService? dbService = null, 
            DeliveryApiClient? deliveryClient = null,
            OrderProcessingOrchestrator? orchestrator = null,
            IMessageBus? messageBus = null)
        {
            _dbService = dbService;
            _deliveryClient = deliveryClient;
            _orchestrator = orchestrator;
            _messageBus = messageBus;
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return;

                if (_dbService != null)
                {
                    Console.WriteLine("[CartApiService] Loading from database...");
                    _produse = await _dbService.LoadProductsFromDatabaseAsync();
                    _persoane = await _dbService.LoadCustomersFromDatabaseAsync();
                    Console.WriteLine($"[CartApiService] Loaded {_produse.Count} products, {_persoane.Count} customers");
                }
                else
                {
                    Console.WriteLine("[CartApiService] Creating sample data...");
                    _produse = CreateSampleProducts();
                    _persoane = CreateSampleCustomers();
                    Console.WriteLine($"[CartApiService] Created {_produse.Count} products, {_persoane.Count} customers");
                }
                
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        private static List<Produs> CreateSampleProducts()
        {
            return new List<Produs>
            {
                new Produs(new CodProdus(1001), "Laptop Dell XPS 15", new UnitQuantity(10), new KilogramQuantity(1.0), new Price(5499.99)),
                new Produs(new CodProdus(1002), "Mouse Logitech MX Master", new UnitQuantity(50), new KilogramQuantity(1.0), new Price(349.99)),
                new Produs(new CodProdus(1003), "Keyboard Mechanical RGB", new UnitQuantity(30), new KilogramQuantity(1.0), new Price(599.99)),
                new Produs(new CodProdus(1004), "Monitor LG 27 4K", new UnitQuantity(15), new KilogramQuantity(1.0), new Price(1899.99)),
                new Produs(new CodProdus(1005), "Laptop Lenovo ThinkPad", new UnitQuantity(8), new KilogramQuantity(1.0), new Price(4299.99))
            };
        }

        private static List<Persoana> CreateSampleCustomers()
        {
            return new List<Persoana>
            {
                new Persoana(new Nume("Ion Popescu"), new EmailP("ion.popescu@example.com"), new Adress("Str. Mihai Eminescu 15"), new List<CosDeCumparaturi>()),
                new Persoana(new Nume("Maria Ionescu"), new EmailP("maria.ionescu@example.com"), new Adress("Bulevardul Unirii 1"), new List<CosDeCumparaturi>()),
                new Persoana(new Nume("Andrei Stanciu"), new EmailP("andrei.stanciu@example.com"), new Adress("Str. Avram Iancu 25"), new List<CosDeCumparaturi>())
            };
        }

        private CosDeCumparaturi GetOrCreateCart(string customerName)
        {
            if (!_activeCarts.ContainsKey(customerName))
            {
                _activeCarts[customerName] = new CosDeCumparaturi();
            }
            return _activeCarts[customerName];
        }

        public async Task<(bool Success, ViewCartResponse? Response, string? Error)> ViewCartAsync(string customerName)
        {
            await EnsureInitializedAsync();
            
            try
            {
                if (string.IsNullOrWhiteSpace(customerName))
                    return (false, null, "Customer name is required");

                var persoana = _persoane!.FirstOrDefault(p => p.Nume.Name == customerName);
                if (persoana == null)
                    return (false, null, $"Customer '{customerName}' not found");

                var cos = GetOrCreateCart(customerName);
                var produseInCos = cos.GetProduseCos() ?? new List<ProdusCos>();

                var items = produseInCos.Select(p => new CartItemDto
                {
                    ProductCode = p.CodProd.Cod,
                    ProductName = p.Nume,
                    Quantity = Convert.ToDecimal(p.Cantitate.Cantitate),
                    UnitPrice = Convert.ToDecimal(p.Price.pret),
                    LineTotal = Convert.ToDecimal(cos.CalculeazaTotalProdus(p)),
                    QuantityType = "Unit",
                    KilogramQuantity = p.Kilogram.CantitateKilogram
                }).ToList();

                var response = new ViewCartResponse
                {
                    CustomerName = customerName,
                    CartStatus = cos.GetStareCos() switch
                    {
                        UnvalidatedCos => "Unvalidated",
                        EmptyCos => "Empty",
                        ValidatedCos => "Validated",
                        PayedCos => "Paid",
                        _ => "Unknown"
                    },
                    Items = items,
                    TotalAmount = Convert.ToDecimal(cos.TotalCos()),
                    TotalItems = items.Count,
                    LastModified = DateTime.UtcNow
                };

                return (true, response, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, AddProductToCartResponse? Response, string? Error)> AddProductToCartAsync(AddProductToCartRequest request)
        {
            await EnsureInitializedAsync();
            
            try
            {
                Console.WriteLine($"\n????????????????????????????????????????????");
                Console.WriteLine($"?  ADD PRODUCT TO CART - DIAGNOSTIC        ?");
                Console.WriteLine($"????????????????????????????????????????????");
                Console.WriteLine($"[DEBUG] Customer: '{request.CustomerName}'");
                Console.WriteLine($"[DEBUG] Product: '{request.ProductName}'");
                Console.WriteLine($"[DEBUG] Quantity: {request.Quantity}");
                
                if (string.IsNullOrWhiteSpace(request.CustomerName))
                    return (false, null, "Customer name is required");
                if (string.IsNullOrWhiteSpace(request.ProductName))
                    return (false, null, "Product name is required");

                Console.WriteLine($"\n[DEBUG] Products in memory: {_produse!.Count}");
                foreach (var p in _produse!)
                {
                    Console.WriteLine($"  - '{p.Nume}' (Code: {p.CodProdus.Cod}, Stock: {p.Quantity.Cantitate})");
                }

                var persoana = _persoane!.FirstOrDefault(p => p.Nume.Name == request.CustomerName);
                if (persoana == null)
                {
                    Console.WriteLine($"[DEBUG] ? Customer not found!");
                    return (false, null, $"Customer '{request.CustomerName}' not found");
                }
                Console.WriteLine($"[DEBUG] ? Customer found: {persoana.Nume.Name}");

                var cos = GetOrCreateCart(request.CustomerName);
                Console.WriteLine($"[DEBUG] Cart state before add: {cos.GetStareCos().GetType().Name}");
                Console.WriteLine($"[DEBUG] Items in cart before: {cos.GetProduseCos().Count}");
                
                Console.WriteLine($"\n[DEBUG] Calling AdaugaProdus with:");
                Console.WriteLine($"  - Product name: '{request.ProductName}'");
                Console.WriteLine($"  - Product list count: {_produse!.Count}");
                
                // CRITICAL: AdaugaProdus modifies the list, so pass it
                cos.AdaugaProdus(request.ProductName, _produse!);
                
                Console.WriteLine($"[DEBUG] Items in cart after: {cos.GetProduseCos().Count}");
                
                // Check if product was actually added
                var produseInCos = cos.GetProduseCos();
                if (produseInCos.Count == 0)
                {
                    Console.WriteLine($"[DEBUG] ? Product was NOT added to cart!");
                    return (false, null, "Product was not added to cart - check console for errors");
                }

                var produsAdaugat = produseInCos.LastOrDefault();
                if (produsAdaugat == null)
                {
                    Console.WriteLine($"[DEBUG] ? Could not find added product in cart");
                    return (false, null, "Product not found after adding");
                }

                Console.WriteLine($"[DEBUG] ? Product added: {produsAdaugat.Nume}");
                Console.WriteLine($"[DEBUG] Cart total: {cos.TotalCos()} RON");

                var addedItem = new CartItemDto
                {
                    ProductCode = produsAdaugat.CodProd.Cod,
                    ProductName = produsAdaugat.Nume,
                    Quantity = (decimal)produsAdaugat.Cantitate.Cantitate,
                    UnitPrice = Convert.ToDecimal(produsAdaugat.Price.pret),
                    LineTotal = Convert.ToDecimal(produsAdaugat.Price.pret * produsAdaugat.Cantitate.Cantitate),
                    QuantityType = "Unit",
                    KilogramQuantity = produsAdaugat.Kilogram.CantitateKilogram
                };

                var response = new AddProductToCartResponse
                {
                    Success = true,
                    Message = $"Product '{request.ProductName}' added successfully",
                    AddedItem = addedItem,
                    NewCartTotal = Convert.ToDecimal(cos.TotalCos()),
                    TotalItems = produseInCos.Count
                };

                Console.WriteLine($"????????????????????????????????????????????\n");

                return (true, response, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] ? Exception: {ex.Message}");
                Console.WriteLine($"[DEBUG] Stack: {ex.StackTrace}");
                return (false, null, $"Error: {ex.Message}");
            }
        }

        public async Task<(bool Success, MarkCartAsPaidResponse? Response, string? Error)> MarkCartAsPaidAsync(MarkCartAsPaidRequest request)
        {
            await EnsureInitializedAsync();
            
            try
            {
                if (string.IsNullOrWhiteSpace(request.CustomerName))
                    return (false, null, "Customer name is required");

                var persoana = _persoane!.FirstOrDefault(p => p.Nume.Name == request.CustomerName);
                if (persoana == null)
                    return (false, null, $"Customer '{request.CustomerName}' not found");

                if (!_activeCarts.ContainsKey(request.CustomerName))
                    return (false, null, "No active cart found");

                var cos = _activeCarts[request.CustomerName];
                var stareCos = cos.GetStareCos();

                if (stareCos is PayedCos)
                    return (false, null, "Cart is already paid");
                if (stareCos is EmptyCos)
                    return (false, null, "Cart is empty");

                var totalAmount = cos.TotalCos();
                var itemCount = cos.GetProduseCos().Count;

                bool paymentSuccess = cos.platesteCos();
                if (!paymentSuccess)
                    return (false, null, "Payment failed");

                // TRIGGER ORDER WORKFLOW WITH ORCHESTRATOR
                if (_orchestrator != null && _dbService != null)
                {
                    try
                    {
                        Console.WriteLine("[CartApiService] Triggering order workflow with event orchestration...");
                        
                        var workflow = new PlasareComandaWorkflow(_dbService, _orchestrator, _messageBus);
                        var orderResult = await workflow.PlaseazaComandaAsync(persoana, cos, _produse!);
                        
                        if (orderResult is ComandaEvent.ComandaPlasataSuccessEvent successEvent)
                        {
                            Console.WriteLine($"[CartApiService] ? Order processed successfully");
                        }
                        else if (orderResult is ComandaEvent.ComandaPlasataFailedEvent failedEvent)
                        {
                            Console.WriteLine($"[CartApiService] ?? Order processing failed: {failedEvent.Reason}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CartApiService] ?? Order workflow error: {ex.Message}");
                    }
                }
                else if (_orchestrator != null)
                {
                    // Even without database, we can trigger the orchestrator
                    try
                    {
                        Console.WriteLine("[CartApiService] Triggering order workflow (no database)...");
                        
                        var workflow = new PlasareComandaWorkflow(null, _orchestrator);
                        var orderResult = await workflow.PlaseazaComandaAsync(persoana, cos, _produse!);
                        
                        if (orderResult is ComandaEvent.ComandaPlasataSuccessEvent successEvent)
                        {
                            Console.WriteLine($"[CartApiService] ? Order processed successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CartApiService] ?? Order workflow error: {ex.Message}");
                    }
                }

                var response = new MarkCartAsPaidResponse
                {
                    Success = true,
                    Message = $"Cart paid successfully for {request.CustomerName}. Invoice and delivery initiated.",
                    TotalPaid = Convert.ToDecimal(totalAmount),
                    ItemsPaid = itemCount,
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = request.TransactionId ?? Guid.NewGuid().ToString()
                };

                return (true, response, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error: {ex.Message}");
            }
        }

        public async Task<Dictionary<string, string>> GetActiveCartsStatusAsync()
        {
            await EnsureInitializedAsync();
            
            return _activeCarts.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GetStareCos() switch
                {
                    UnvalidatedCos => "Unvalidated",
                    EmptyCos => "Empty",
                    ValidatedCos => "Validated",
                    PayedCos => "Paid",
                    _ => "Unknown"
                }
            );
        }
    }
}
