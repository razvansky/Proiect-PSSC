using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lucrarea1PSSC.clase.Infrastructure.Database
{
    /// <summary>
    /// Database configuration and initialization helper
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// Configure services with database context
        /// </summary>
        public static IServiceCollection AddECommerceDatabase(
            this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<ECommerceDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            services.AddScoped<UnitOfWork>();
            services.AddScoped<OrderWorkflowDatabaseService>();

            return services;
        }

        /// <summary>
        /// Initialize database (create if not exists)
        /// </summary>
        public static async System.Threading.Tasks.Task InitializeDatabaseAsync(
            this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
                try
                {
                    // Create database if it doesn't exist
                    await context.Database.EnsureCreatedAsync();
                    Console.WriteLine("[DB] Database initialized successfully");

                    // Optionally migrate if using migrations
                    // await context.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB ERROR] Failed to initialize database: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Seed initial data if database is empty
        /// </summary>
        public static async System.Threading.Tasks.Task SeedDatabaseAsync(
            this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
                try
                {
                    // Check if already seeded
                    if (await context.Products.CountAsync() > 0)
                    {
                        Console.WriteLine("[DB] Database already contains data, skipping seed");
                        return;
                    }

                    // Seed products
                    var products = new ProductDbModel[]
                    {
                        new ProductDbModel { Code = 1001, Name = "Laptop Dell XPS 15", Price = 5499.99m, QuantityInStock = 10 },
                        new ProductDbModel { Code = 1002, Name = "Mouse Logitech MX Master", Price = 349.99m, QuantityInStock = 50 },
                        new ProductDbModel { Code = 1003, Name = "Keyboard Mechanical RGB", Price = 599.99m, QuantityInStock = 30 },
                        new ProductDbModel { Code = 1004, Name = "Monitor LG 27\" 4K", Price = 1899.99m, QuantityInStock = 15 },
                        new ProductDbModel { Code = 1005, Name = "Laptop Lenovo ThinkPad", Price = 4299.99m, QuantityInStock = 8 },
                        new ProductDbModel { Code = 1006, Name = "Webcam Logitech C920", Price = 449.99m, QuantityInStock = 25 },
                        new ProductDbModel { Code = 1007, Name = "Headset HyperX Cloud II", Price = 499.99m, QuantityInStock = 40 },
                        new ProductDbModel { Code = 1008, Name = "SSD Samsung 1TB", Price = 699.99m, QuantityInStock = 60 },
                        new ProductDbModel { Code = 1009, Name = "RAM Corsair 16GB DDR4", Price = 399.99m, QuantityInStock = 45 },
                        new ProductDbModel { Code = 1010, Name = "GPU RTX 4070", Price = 3999.99m, QuantityInStock = 5 }
                    };

                    context.Products.AddRange(products);

                    // Seed customers
                    var customers = new CustomerDbModel[]
                    {
                        new CustomerDbModel { Code = "CUST001", Name = "Ion Popescu", Email = "ion.popescu@example.com", Address = "Str. Mihai Eminescu 15", City = "Cluj-Napoca", PostalCode = "400347", Phone = "0721234567" },
                        new CustomerDbModel { Code = "CUST002", Name = "Maria Ionescu", Email = "maria.ionescu@example.com", Address = "Bulevardul Unirii 1", City = "Bucure?ti", PostalCode = "030823", Phone = "0732345678" },
                        new CustomerDbModel { Code = "CUST003", Name = "Andrei Stanciu", Email = "andrei.stanciu@example.com", Address = "Str. Avram Iancu 25", City = "Timi?oara", PostalCode = "300088", Phone = "0743456789" },
                        new CustomerDbModel { Code = "CUST004", Name = "Elena Radu", Email = "elena.radu@example.com", Address = "Str. Republicii 45", City = "Bra?ov", PostalCode = "500030", Phone = "0754567890" },
                        new CustomerDbModel { Code = "CUST005", Name = "Mihai Popa", Email = "mihai.popa@example.com", Address = "Str. Victoriei 10", City = "Ia?i", PostalCode = "700028", Phone = "0765678901" }
                    };

                    context.Customers.AddRange(customers);
                    await context.SaveChangesAsync();

                    Console.WriteLine("[DB] Database seeded with initial data");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DB ERROR] Failed to seed database: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
