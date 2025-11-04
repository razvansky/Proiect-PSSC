using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.Infrastructure;
using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.api.DTOs;

namespace Lucrarea1PSSC.api.Services
{
    /// <summary>
    /// Service for managing shopping cart operations via API
    /// </summary>
    public class CartApiService
    {
        private readonly OrderWorkflowDatabaseService _dbService;
        private static List<Produs>? _produse;
        private static List<Persoana>? _persoane;
        private static readonly Dictionary<string, CosDeCumparaturi> _activeCarts = new();
        private static bool _initialized = false;
        private static readonly SemaphoreSlim _initLock = new(1, 1);

        public CartApiService(OrderWorkflowDatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Ensure data is loaded from database
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return;

                Console.WriteLine("[CartApiService] Loading products and customers from database...");
                _produse = await _dbService.LoadProductsFromDatabaseAsync();
                _persoane = await _dbService.LoadCustomersFromDatabaseAsync();
                Console.WriteLine($"[CartApiService] Loaded {_produse.Count} products and {_persoane.Count} customers");
                
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Get or create cart for customer
        /// </summary>
        private CosDeCumparaturi GetOrCreateCart(string customerName)
        {
            if (!_activeCarts.ContainsKey(customerName))
            {
                var persoana = _persoane!.FirstOrDefault(p => p.Nume.Name == customerName);
                if (persoana?.CosCurent != null)
                {
                    _activeCarts[customerName] = persoana.CosCurent;
                }
                else
                {
                    _activeCarts[customerName] = new CosDeCumparaturi();
                    
                    // Update person with cart
                    if (persoana != null)
                    {
                        var updatedPersoana = persoana.AdaugaCos(_activeCarts[customerName]);
                        var index = _persoane!.IndexOf(persoana);
                        _persoane[index] = updatedPersoana;

                        // Publish event
                        EventBus.Publish(new CartEvents.CosCreatEvent(
                            customerName,
                            DateTime.UtcNow
                        ));
                    }
                }
            }
            return _activeCarts[customerName];
        }

        /// <summary>
        /// View shopping cart
        /// </summary>
        public async Task<(bool Success, ViewCartResponse? Response, string? Error)> ViewCartAsync(string customerName)
        {
            await EnsureInitializedAsync();
            
            try
            {
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    return (false, null, "Customer name is required");
                }

                var persoana = _persoane!.FirstOrDefault(p => p.Nume.Name == customerName);
                if (persoana == null)
                {
                    return (false, null, $"Customer '{customerName}' not found");
                }

                var cos = GetOrCreateCart(customerName);
                var stareCos = cos.GetStareCos();
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
                    CartStatus = stareCos switch
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
                return (false, null, $"Error viewing cart: {ex.Message}");
            }
        }

        /// <summary>
        /// Add product to cart
        /// </summary>
        public async Task<(bool Success, AddProductToCartResponse? Response, string? Error)> AddProductToCartAsync(
            AddProductToCartRequest request)
        {
            await EnsureInitializedAsync();
            
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.CustomerName))
                {
                    return (false, null, "Customer name is required");
                }

                if (string.IsNullOrWhiteSpace(request.ProductName))
                {
                    return (false, null, "Product name is required");
                }

                // Validate customer exists
                var persoana = _persoane!.FirstOrDefault(p => p.Nume.Name == request.CustomerName);
                if (persoana == null)
                {
                    return (false, null, $"Customer '{request.CustomerName}' not found");
                }

                // Verify product exists in database
                var (productExists, dbProduct) = await _dbService.VerifyProductExistsAsync(request.ProductName);
                if (!productExists)
                {
                    return (false, null, $"Product '{request.ProductName}' not found in database");
                }

                // Check stock availability
                var (hasStock, availableStock) = await _dbService.CheckProductStockAsync(
                    dbProduct!.Code,
                    request.Quantity);

                if (!hasStock)
                {
                    return (false, null, 
                        $"Insufficient stock for '{request.ProductName}'. Available: {availableStock}");
                }

                // Get or create cart
                var cos = GetOrCreateCart(request.CustomerName);

                // Add product to cart
                cos.AdaugaProdus(request.ProductName, _produse!);

                // Find added product
                var produsAdaugat = _produse!.FirstOrDefault(p => p.Nume == request.ProductName);
                if (produsAdaugat == null)
                {
                    return (false, null, "Product not found in inventory");
                }

                // Publish event
                EventBus.Publish(new CartEvents.ProdusAdaugatInCosEvent(
                    produsAdaugat.Nume,
                    produsAdaugat.CodProdus.Cod,
                    1,
                    produsAdaugat.Pret.pret,
                    DateTime.UtcNow
                ));

                // Create response
                var addedItem = new CartItemDto
                {
                    ProductCode = produsAdaugat.CodProdus.Cod,
                    ProductName = produsAdaugat.Nume,
                    Quantity = 1,
                    UnitPrice = Convert.ToDecimal(produsAdaugat.Pret.pret),
                    LineTotal = Convert.ToDecimal(produsAdaugat.Pret.pret),
                    QuantityType = "Unit",
                    KilogramQuantity = produsAdaugat.Kilogram.CantitateKilogram
                };

                var response = new AddProductToCartResponse
                {
                    Success = true,
                    Message = $"Product '{request.ProductName}' added to cart successfully",
                    AddedItem = addedItem,
                    NewCartTotal = Convert.ToDecimal(cos.TotalCos()),
                    TotalItems = cos.GetProduseCos().Count
                };

                return (true, response, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error adding product to cart: {ex.Message}");
            }
        }

        /// <summary>
        /// Mark cart as paid
        /// </summary>
        public async Task<(bool Success, MarkCartAsPaidResponse? Response, string? Error)> MarkCartAsPaidAsync(
            MarkCartAsPaidRequest request)
        {
            await EnsureInitializedAsync();
            
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.CustomerName))
                {
                    return (false, null, "Customer name is required");
                }

                // Validate customer exists
                var persoana = _persoane!.FirstOrDefault(p => p.Nume.Name == request.CustomerName);
                if (persoana == null)
                {
                    return (false, null, $"Customer '{request.CustomerName}' not found");
                }

                // Get cart
                if (!_activeCarts.ContainsKey(request.CustomerName))
                {
                    return (false, null, "No active cart found for customer");
                }

                var cos = _activeCarts[request.CustomerName];

                // Validate cart state
                var stareCos = cos.GetStareCos();
                if (stareCos is PayedCos)
                {
                    return (false, null, "Cart is already paid");
                }

                if (stareCos is UnvalidatedCos)
                {
                    return (false, null, "Cart is not validated");
                }

                if (stareCos is EmptyCos)
                {
                    return (false, null, "Cart is empty. Add products before payment");
                }

                // Get cart details before payment
                var totalAmount = cos.TotalCos();
                var itemCount = cos.GetProduseCos().Count;

                // Process payment
                bool paymentSuccess = cos.platesteCos();

                if (!paymentSuccess)
                {
                    return (false, null, "Payment failed. Please try again");
                }

                var paymentDate = DateTime.UtcNow;

                // Publish event
                EventBus.Publish(new CartEvents.CosPlatitEvent(
                    request.CustomerName,
                    totalAmount,
                    itemCount,
                    paymentDate
                ));

                // Create response
                var response = new MarkCartAsPaidResponse
                {
                    Success = true,
                    Message = $"Cart paid successfully for {request.CustomerName}",
                    TotalPaid = Convert.ToDecimal(totalAmount),
                    ItemsPaid = itemCount,
                    PaymentDate = paymentDate,
                    TransactionId = request.TransactionId ?? Guid.NewGuid().ToString()
                };

                return (true, response, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error processing payment: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all active carts (for admin/debugging)
        /// </summary>
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
