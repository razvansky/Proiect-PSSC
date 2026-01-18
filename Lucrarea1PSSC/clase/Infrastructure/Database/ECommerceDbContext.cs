using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lucrarea1PSSC.clase.Infrastructure.Database
{
    /// <summary>
    /// Database model for Product
    /// Maps to dbo.Products table
    /// </summary>
    public class ProductDbModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal QuantityInStock { get; set; }
        public string QuantityType { get; set; } = "Unit";
        public double KilogramQuantity { get; set; } = 1.0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<OrderItemDbModel> OrderItems { get; set; } = new List<OrderItemDbModel>();
    }

    /// <summary>
    /// Database model for Customer
    /// Maps to dbo.Customers table
    /// </summary>
    public class CustomerDbModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = "Romania";
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalSpent { get; set; } = 0;
        public int LoyaltyPoints { get; set; } = 0;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastPurchaseAt { get; set; }

        // Navigation
        public virtual ICollection<OrderDbModel> Orders { get; set; } = new List<OrderDbModel>();
    }

    /// <summary>
    /// Database model for Order
    /// Maps to dbo.Orders table
    /// </summary>
    public class OrderDbModel
    {
        public int Id { get; set; }
        public Guid OrderNumber { get; set; } = Guid.NewGuid();
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? DeliveryCity { get; set; }
        public string? DeliveryPostalCode { get; set; }
        public string DeliveryCountry { get; set; } = "Romania";
        
        public int CustomerId { get; set; }
        
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; } = 0;
        public decimal Tax { get; set; } = 0;
        public decimal Total { get; set; }
        
        public string Status { get; set; } = "Placed"; // Placed, Shipped, Delivered, Cancelled
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        
        public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        
        public string? Notes { get; set; }

        // Navigation
        public virtual CustomerDbModel Customer { get; set; } = null!;
        public virtual ICollection<OrderItemDbModel> OrderItems { get; set; } = new List<OrderItemDbModel>();
    }

    /// <summary>
    /// Database model for OrderItem
    /// Maps to dbo.OrderItems table
    /// </summary>
    public class OrderItemDbModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int ProductCode { get; set; }
        public string ProductName { get; set; } = string.Empty;
        
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        
        public string QuantityType { get; set; } = "Unit";

        // Navigation
        public virtual OrderDbModel Order { get; set; } = null!;
        public virtual ProductDbModel Product { get; set; } = null!;
    }

    /// <summary>
    /// EF Core DbContext for E-Commerce application
    /// Handles all database operations
    /// </summary>
    public class ECommerceDbContext : DbContext
    {
        public const string ConnectionStringName = "DefaultConnection";

        public DbSet<ProductDbModel> Products { get; set; }
        public DbSet<CustomerDbModel> Customers { get; set; }
        public DbSet<OrderDbModel> Orders { get; set; }
        public DbSet<OrderItemDbModel> OrderItems { get; set; }

        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Products
            modelBuilder.Entity<ProductDbModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Name);
                
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.QuantityInStock).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.QuantityType).HasMaxLength(20).HasDefaultValue("Unit");
                
                entity.ToTable("Products");
            });

            // Customers
            modelBuilder.Entity<CustomerDbModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Name);
                
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.TotalSpent).HasColumnType("decimal(18, 2)");
                
                entity.HasMany(e => e.Orders)
                    .WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.ToTable("Customers");
            });

            // Orders
            modelBuilder.Entity<OrderDbModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.OrderDate);
                entity.HasIndex(e => e.Status);
                
                entity.Property(e => e.DeliveryAddress).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.ShippingCost).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Tax).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Placed");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                
                entity.HasMany(e => e.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.ToTable("Orders");
            });

            // OrderItems
            modelBuilder.Entity<OrderItemDbModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.ProductId);
                
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.LineTotal).HasColumnType("decimal(18, 2)");
                
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.ToTable("OrderItems");
            });
        }
    }
}
