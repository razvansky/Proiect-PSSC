using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.ClaseCos;

namespace Lucrarea1PSSC.clase.Infrastructure.Database
{
    /// <summary>
    /// Service for order workflow database operations
    /// Coordinates database interactions for the order placement workflow
    /// </summary>
    public class OrderWorkflowDatabaseService
    {
        private readonly UnitOfWork _unitOfWork;

        public OrderWorkflowDatabaseService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        /// <summary>
        /// Load products from database
        /// Converts database models to domain models
        /// </summary>
        public async Task<List<Produs>> LoadProductsFromDatabaseAsync()
        {
            try
            {
                var dbProducts = await _unitOfWork.Products.GetAllProductsAsync();
                var products = new List<Produs>();

                foreach (var dbProduct in dbProducts)
                {
                    var produs = new Produs(
                        new CodProdus(dbProduct.Code),
                        dbProduct.Name,
                        new UnitQuantity((double)dbProduct.QuantityInStock),
                        new KilogramQuantity(dbProduct.KilogramQuantity),
                        new Price((double)dbProduct.Price)
                    );
                    products.Add(produs);
                }

                Console.WriteLine($"[DB] Loaded {products.Count} products from database");
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to load products: {ex.Message}");
                return new List<Produs>();
            }
        }

        /// <summary>
        /// Load customers from database
        /// Converts database models to domain models
        /// </summary>
        public async Task<List<Persoana>> LoadCustomersFromDatabaseAsync()
        {
            try
            {
                var dbCustomers = await _unitOfWork.Customers.GetAllCustomersAsync();
                var customers = new List<Persoana>();

                foreach (var dbCustomer in dbCustomers)
                {
                    var persoana = new Persoana(
                        new Nume(dbCustomer.Name),
                        new EmailP(dbCustomer.Email),
                        new Adress(dbCustomer.Address),
                        new List<CosDeCumparaturi>()
                    );
                    customers.Add(persoana);
                }

                Console.WriteLine($"[DB] Loaded {customers.Count} customers from database");
                return customers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to load customers: {ex.Message}");
                return new List<Persoana>();
            }
        }

        /// <summary>
        /// Verify product exists in database
        /// </summary>
        public async Task<(bool Exists, ProductDbModel? Product)> VerifyProductExistsAsync(string productName)
        {
            try
            {
                var product = await _unitOfWork.Products.GetProductByNameAsync(productName);
                if (product != null)
                {
                    Console.WriteLine($"[DB] Product found: {productName} (Code: {product.Code}, Stock: {product.QuantityInStock})");
                    return (true, product);
                }

                Console.WriteLine($"[DB WARNING] Product not found: {productName}");
                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Error verifying product: {ex.Message}");
                return (false, null);
            }
        }

        /// <summary>
        /// Check product stock availability
        /// </summary>
        public async Task<(bool HasStock, decimal AvailableQuantity)> CheckProductStockAsync(int productCode, decimal requiredQuantity)
        {
            try
            {
                var hasStock = await _unitOfWork.Products.HasSufficientStockAsync(productCode, requiredQuantity);
                var availableStock = await _unitOfWork.Products.GetStockLevelAsync(productCode);

                if (hasStock)
                {
                    Console.WriteLine($"[DB] Stock available for product {productCode}: {availableStock} units");
                    return (true, availableStock);
                }

                Console.WriteLine($"[DB WARNING] Insufficient stock for product {productCode}: Have {availableStock}, Need {requiredQuantity}");
                return (false, availableStock);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Error checking stock: {ex.Message}");
                return (false, 0);
            }
        }

        /// <summary>
        /// Process order placement in database
        /// Saves order and order items, updates stock
        /// </summary>
        public async Task<(bool Success, OrderDbModel Order, string Error)> PlaceOrderInDatabaseAsync(
            Guid orderNumber,
            Persoana customer,
            CosDeCumparaturi cart,
            List<Produs> products)
        {
            try
            {
                // Get or create customer in database
                var dbCustomer = await _unitOfWork.Customers.GetCustomerByNameAsync(customer.Nume.Name);
                if (dbCustomer == null)
                {
                    return (false, null, $"Customer {customer.Nume.Name} not found in database");
                }

                // Get cart details
                var cartItems = cart.GetProduseCos();
                if (cartItems == null || cartItems.Count == 0)
                {
                    return (false, null, "Cart is empty");
                }

                var cartTotal = cart.TotalCos();
                if (cartTotal <= 0)
                {
                    return (false, null, "Cart total is invalid");
                }

                // Calculate costs
                decimal subtotal = (decimal)cartTotal;
                decimal shippingCost = subtotal > 500 ? 0 : 20; // Free shipping over 500 RON
                decimal tax = subtotal * 0.19m; // 19% VAT

                // Create order in database
                var dbOrder = await _unitOfWork.Orders.CreateOrderAsync(
                    orderNumber,
                    dbCustomer.Id,
                    customer.Adress.adress,
                    "Romania", // Default city
                    "000000",  // Default postal code
                    subtotal,
                    shippingCost,
                    tax,
                    "Manual",  // Payment method
                    orderNumber.ToString()
                );

                Console.WriteLine($"[DB] Order {orderNumber} created with ID {dbOrder.Id}");

                // Add order items and decrease stock
                foreach (var cartItem in cartItems)
                {
                    // Find product in database
                    var dbProduct = await _unitOfWork.Products.GetProductByNameAsync(cartItem.Nume);
                    if (dbProduct == null)
                    {
                        return (false, null, $"Product {cartItem.Nume} not found in database");
                    }

                    // Add order item
                    await _unitOfWork.Orders.AddOrderItemAsync(
                        dbOrder.Id,
                        dbProduct.Id,
                        dbProduct.Code,
                        dbProduct.Name,
                        (decimal)cartItem.Cantitate.Cantitate,
                        dbProduct.Price,
                        "Unit"
                    );

                    // Decrease stock
                    var stockDecreased = await _unitOfWork.Products.DecreaseStockAsync(
                        dbProduct.Code,
                        (decimal)cartItem.Cantitate.Cantitate
                    );

                    if (!stockDecreased)
                    {
                        return (false, null, $"Failed to decrease stock for product {cartItem.Nume}");
                    }

                    Console.WriteLine($"[DB] Stock decreased for {cartItem.Nume}: -{cartItem.Cantitate.Cantitate} units");
                }

                // Update customer statistics
                await _unitOfWork.Customers.UpdateLoyaltyPointsAsync(
                    dbCustomer.Id,
                    (int)(subtotal / 10) // 1 point per 10 RON
                );

                // Save all changes
                await _unitOfWork.SaveChangesAsync();

                Console.WriteLine($"[DB] Order {orderNumber} successfully saved to database");
                return (true, dbOrder, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to place order: {ex.Message}");
                return (false, null, $"Database error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get order details from database
        /// </summary>
        public async Task<OrderDbModel?> GetOrderDetailsAsync(Guid orderNumber)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetOrderByNumberAsync(orderNumber);
                if (order != null)
                {
                    Console.WriteLine($"[DB] Retrieved order {orderNumber}: {order.OrderItems.Count} items, Total: {order.Total} RON");
                }
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to get order: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get customer order history
        /// </summary>
        public async Task<List<OrderDbModel>> GetCustomerOrderHistoryAsync(string customerName)
        {
            try
            {
                var dbCustomer = await _unitOfWork.Customers.GetCustomerByNameAsync(customerName);
                if (dbCustomer == null)
                {
                    return new List<OrderDbModel>();
                }

                var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(dbCustomer.Id);
                Console.WriteLine($"[DB] Retrieved {orders.Count} orders for customer {customerName}");
                return orders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to get customer order history: {ex.Message}");
                return new List<OrderDbModel>();
            }
        }

        /// <summary>
        /// Rollback order (decrease stock back, delete order)
        /// </summary>
        public async Task<bool> RollbackOrderAsync(OrderDbModel order)
        {
            try
            {
                if (order?.OrderItems == null || order.OrderItems.Count == 0)
                    return false;

                // Increase stock for each item
                foreach (var item in order.OrderItems)
                {
                    await _unitOfWork.Products.IncreaseStockAsync(item.ProductCode, item.Quantity);
                }

                // Mark order as cancelled
                await _unitOfWork.Orders.UpdateOrderStatusAsync(order.Id, "Cancelled");

                Console.WriteLine($"[DB] Order {order.OrderNumber} rolled back");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB ERROR] Failed to rollback order: {ex.Message}");
                return false;
            }
        }
    }
}
