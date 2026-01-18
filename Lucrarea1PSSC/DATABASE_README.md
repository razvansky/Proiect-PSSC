# Database Integration for E-Commerce DDD Application

## Overview
This document provides instructions for setting up and using the SQL Server database integration for the order placement workflow.

## Prerequisites
- SQL Server (LocalDB, Express, or Full version)
- .NET 9 SDK
- Visual Studio 2022 or later

## Database Setup

### Option 1: Using SQL Script (Recommended)

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Open the file `Lucrarea1PSSC\database\ECommerceDatabaseSchema.sql`
4. Execute the script (F5)
5. The script will:
   - Create database `ECommerceDB`
   - Create tables: Products, Customers, Orders, OrderItems
   - Insert sample data
   - Create stored procedures and views

### Option 2: Using Entity Framework Core

The application will automatically create the database on first run if it doesn't exist.

## Connection String

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ECommerceDB;Trusted_Connection=true;Encrypt=false;MultipleActiveResultSets=true"
  }
}
```

**Connection String Options:**
- `Server=.` - Local SQL Server instance
- `Server=(localdb)\\mssqllocaldb` - SQL Server LocalDB
- `Server=YOUR_SERVER_NAME` - Remote SQL Server

## Database Schema

### Tables

#### Products
| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key (Identity) |
| Code | INT | Unique product code |
| Name | NVARCHAR(100) | Product name |
| Price | DECIMAL(18,2) | Product price |
| QuantityInStock | DECIMAL(18,2) | Available stock |
| QuantityType | NVARCHAR(20) | Unit or Kilogram |
| IsActive | BIT | Active status |

#### Customers
| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key (Identity) |
| Code | NVARCHAR(50) | Unique customer code |
| Name | NVARCHAR(200) | Customer name |
| Email | NVARCHAR(255) | Customer email |
| Address | NVARCHAR(500) | Delivery address |
| TotalOrders | INT | Order count |
| TotalSpent | DECIMAL(18,2) | Total spent |
| LoyaltyPoints | INT | Loyalty points |

#### Orders
| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key (Identity) |
| OrderNumber | UNIQUEIDENTIFIER | Unique order number |
| CustomerId | INT | Foreign Key to Customers |
| DeliveryAddress | NVARCHAR(500) | Delivery address |
| Subtotal | DECIMAL(18,2) | Order subtotal |
| ShippingCost | DECIMAL(18,2) | Shipping cost |
| Tax | DECIMAL(18,2) | Tax amount |
| Total | DECIMAL(18,2) | Total amount |
| Status | NVARCHAR(50) | Order status |
| PlacedAt | DATETIME2 | Order date |

#### OrderItems
| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key (Identity) |
| OrderId | INT | Foreign Key to Orders |
| ProductId | INT | Foreign Key to Products |
| ProductCode | INT | Product code snapshot |
| ProductName | NVARCHAR(100) | Product name snapshot |
| Quantity | DECIMAL(18,2) | Quantity ordered |
| UnitPrice | DECIMAL(18,2) | Price per unit |
| LineTotal | DECIMAL(18,2) | Line total |

## Usage

### Loading Data from Database

The application automatically loads products and customers from the database on startup:

```csharp
// Products and customers are loaded from database
List<Produs> produse = await dbService.LoadProductsFromDatabaseAsync();
List<Persoana> persoane = await dbService.LoadCustomersFromDatabaseAsync();
```

### Placing an Order

When placing an order (Option 10 in menu):

1. **Validation**: System validates customer and cart
2. **Stock Check**: Verifies product availability in database
3. **Order Creation**: Creates order in database
4. **Stock Update**: Decreases product stock
5. **Event Publishing**: Publishes success/failure events

### Database Operations

#### Check Product Availability
```csharp
var (exists, product) = await dbService.VerifyProductExistsAsync("Laptop");
var (hasStock, quantity) = await dbService.CheckProductStockAsync(1001, 2);
```

#### Place Order
```csharp
var (success, order, error) = await dbService.PlaceOrderInDatabaseAsync(
    orderNumber,
    customer,
    cart,
    products
);
```

## Features

### Automatic Stock Management
- Stock decreases when products added to cart
- Stock increases when products removed
- Stock validation before order placement
- Automatic stock rollback on order cancellation

### Customer Statistics
- Total orders counter updated automatically
- Total spent calculated automatically
- Loyalty points awarded (1 point per 10 RON)
- Last purchase date tracked

### Order Tracking
- Unique order number (GUID)
- Order status tracking (Placed ? Shipped ? Delivered)
- Order history per customer
- Detailed order items

## Troubleshooting

### Cannot Connect to Database

**Error**: `A network-related or instance-specific error occurred`

**Solutions**:
1. Verify SQL Server is running
2. Check connection string
3. Enable TCP/IP protocol in SQL Server Configuration Manager
4. Check firewall settings

### Database Does Not Exist

**Error**: `Cannot open database "ECommerceDB"`

**Solutions**:
1. Run the SQL schema script
2. Let EF Core create database automatically on first run
3. Check database name in connection string

### Package Not Found

**Error**: `Could not find package 'Microsoft.EntityFrameworkCore.SqlServer'`

**Solution**:
```bash
dotnet restore
```

## Maintenance

### View Orders
```sql
SELECT * FROM vw_OrderSummary
ORDER BY OrderDate DESC
```

### Check Stock Levels
```sql
SELECT Code, Name, QuantityInStock, 
       CASE WHEN QuantityInStock < 5 THEN 'Low Stock' 
            WHEN QuantityInStock = 0 THEN 'Out of Stock'
            ELSE 'In Stock' 
       END AS StockStatus
FROM Products
WHERE IsActive = 1
ORDER BY QuantityInStock
```

### Customer Activity
```sql
SELECT c.Name, c.Email, c.TotalOrders, c.TotalSpent, c.LoyaltyPoints,
       c.LastPurchaseAt, 
       DATEDIFF(day, c.LastPurchaseAt, GETUTCDATE()) AS DaysSinceLastOrder
FROM Customers c
WHERE c.IsActive = 1
ORDER BY c.TotalSpent DESC
```

## Sample Data

The database script includes sample data:
- **10 Products**: Laptops, peripherals, components
- **5 Customers**: Test customers with different addresses

## Architecture

```
Program.cs
    ?
OrderWorkflowDatabaseService
    ?
UnitOfWork
    ??? ProductRepository
    ??? CustomerRepository
    ??? OrderRepository
         ?
    ECommerceDbContext
         ?
    SQL Server Database
```

## Production Considerations

1. **Connection Pooling**: Enabled by default
2. **Transaction Management**: Use `UnitOfWork.SaveChangesAsync()`
3. **Error Handling**: All database operations wrapped in try-catch
4. **Logging**: Console logging for database operations
5. **Performance**: Indexes on frequently queried columns

## Next Steps

1. Run the SQL schema script
2. Update connection string
3. Build and run the application
4. Test order placement workflow
5. Verify data in database

---

**Note**: This is a development/learning setup. For production, consider:
- Moving connection string to Azure Key Vault or user secrets
- Implementing proper migration strategy
- Adding comprehensive logging (Serilog, NLog)
- Implementing retry policies (Polly)
- Adding database health checks
- Implementing data archiving strategy
