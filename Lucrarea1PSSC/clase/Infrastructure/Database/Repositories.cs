using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;

namespace Lucrarea1PSSC.clase.Infrastructure.Database
{
    /// <summary>
    /// Repository for product operations
    /// Provides database access for product information and stock management
    /// </summary>
    public class ProductRepository
    {
        private readonly ECommerceDbContext _context;

        public ProductRepository(ECommerceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Get all active products from database
        /// </summary>
        public async Task<List<ProductDbModel>> GetAllProductsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get product by code
        /// </summary>
        public async Task<ProductDbModel?> GetProductByCodeAsync(int code)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Code == code && p.IsActive);
        }

        /// <summary>
        /// Get product by name
        /// </summary>
        public async Task<ProductDbModel?> GetProductByNameAsync(string name)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Name == name && p.IsActive);
        }

        /// <summary>
        /// Check if product exists by code
        /// </summary>
        public async Task<bool> ProductExistsAsync(int code)
        {
            return await _context.Products
                .AnyAsync(p => p.Code == code && p.IsActive);
        }

        /// <summary>
        /// Check if product has sufficient stock
        /// </summary>
        public async Task<bool> HasSufficientStockAsync(int productCode, decimal quantity)
        {
            var product = await GetProductByCodeAsync(productCode);
            return product != null && product.QuantityInStock >= quantity;
        }

        /// <summary>
        /// Decrease product stock
        /// </summary>
        public async Task<bool> DecreaseStockAsync(int productCode, decimal quantity)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Code == productCode);

                if (product == null)
                    return false;

                if (product.QuantityInStock < quantity)
                    return false;

                product.QuantityInStock -= quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Increase product stock
        /// </summary>
        public async Task<bool> IncreaseStockAsync(int productCode, decimal quantity)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Code == productCode);

                if (product == null)
                    return false;

                product.QuantityInStock += quantity;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get product stock level
        /// </summary>
        public async Task<decimal> GetStockLevelAsync(int productCode)
        {
            var product = await GetProductByCodeAsync(productCode);
            return product?.QuantityInStock ?? 0;
        }
    }

    /// <summary>
    /// Repository for customer operations
    /// Provides database access for customer information
    /// </summary>
    public class CustomerRepository
    {
        private readonly ECommerceDbContext _context;

        public CustomerRepository(ECommerceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Get all active customers
        /// </summary>
        public async Task<List<CustomerDbModel>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get customer by code
        /// </summary>
        public async Task<CustomerDbModel?> GetCustomerByCodeAsync(string code)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
        }

        /// <summary>
        /// Get customer by email
        /// </summary>
        public async Task<CustomerDbModel?> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Email == email && c.IsActive);
        }

        /// <summary>
        /// Get customer by name
        /// </summary>
        public async Task<CustomerDbModel?> GetCustomerByNameAsync(string name)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Name == name && c.IsActive);
        }

        /// <summary>
        /// Check if customer exists
        /// </summary>
        public async Task<bool> CustomerExistsAsync(string code)
        {
            return await _context.Customers
                .AnyAsync(c => c.Code == code && c.IsActive);
        }

        /// <summary>
        /// Create new customer
        /// </summary>
        public async Task<CustomerDbModel> CreateCustomerAsync(
            string code, string name, string email, string address,
            string? city, string? postalCode, string? phone = null)
        {
            var customer = new CustomerDbModel
            {
                Code = code,
                Name = name,
                Email = email,
                Address = address,
                City = city,
                PostalCode = postalCode,
                Phone = phone,
                IsActive = true,
                RegisteredAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        /// <summary>
        /// Update customer loyalty points
        /// </summary>
        public async Task<bool> UpdateLoyaltyPointsAsync(int customerId, int points)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                    return false;

                customer.LoyaltyPoints += points;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Repository for order operations
    /// Provides database access for order management
    /// </summary>
    public class OrderRepository
    {
        private readonly ECommerceDbContext _context;

        public OrderRepository(ECommerceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Create new order
        /// </summary>
        public async Task<OrderDbModel> CreateOrderAsync(
            Guid orderNumber,
            int customerId,
            string deliveryAddress,
            string? deliveryCity,
            string? deliveryPostalCode,
            decimal subtotal,
            decimal shippingCost,
            decimal tax,
            string? paymentMethod = "CreditCard",
            string? transactionId = null)
        {
            try
            {
                var order = new OrderDbModel
                {
                    OrderNumber = orderNumber,
                    CustomerId = customerId,
                    DeliveryAddress = deliveryAddress,
                    DeliveryCity = deliveryCity,
                    DeliveryPostalCode = deliveryPostalCode,
                    Subtotal = subtotal,
                    ShippingCost = shippingCost,
                    Tax = tax,
                    Total = subtotal + shippingCost + tax,
                    PaymentMethod = paymentMethod,
                    TransactionId = transactionId,
                    Status = "Placed",
                    OrderDate = DateTime.UtcNow,
                    PlacedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return order;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Add item to order
        /// </summary>
        public async Task<OrderItemDbModel> AddOrderItemAsync(
            int orderId,
            int productId,
            int productCode,
            string productName,
            decimal quantity,
            decimal unitPrice,
            string quantityType = "Unit")
        {
            try
            {
                var orderItem = new OrderItemDbModel
                {
                    OrderId = orderId,
                    ProductId = productId,
                    ProductCode = productCode,
                    ProductName = productName,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    LineTotal = quantity * unitPrice,
                    QuantityType = quantityType
                };

                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();

                return orderItem;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error adding order item: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get order by number
        /// </summary>
        public async Task<OrderDbModel?> GetOrderByNumberAsync(Guid orderNumber)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        /// <summary>
        /// Get orders by customer
        /// </summary>
        public async Task<List<OrderDbModel>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    return false;

                order.Status = status;
                if (status == "Shipped" && !order.ShippedAt.HasValue)
                    order.ShippedAt = DateTime.UtcNow;
                else if (status == "Delivered" && !order.DeliveredAt.HasValue)
                    order.DeliveredAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        public async Task<List<OrderDbModel>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }

    /// <summary>
    /// Unit of Work pattern implementation for database operations
    /// Coordinates all repositories
    /// </summary>
    public class UnitOfWork : IDisposable
    {
        private readonly ECommerceDbContext _context;
        private ProductRepository? _productRepository;
        private CustomerRepository? _customerRepository;
        private OrderRepository? _orderRepository;

        public UnitOfWork(ECommerceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ProductRepository Products =>
            _productRepository ??= new ProductRepository(_context);

        public CustomerRepository Customers =>
            _customerRepository ??= new CustomerRepository(_context);

        public OrderRepository Orders =>
            _orderRepository ??= new OrderRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
