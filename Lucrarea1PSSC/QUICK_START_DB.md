# Quick Start Guide - Database Integration

## What Was Implemented

? **Requirement 1**: Load state from database  
? **Requirement 2**: Verify products/stock from database  
? **Requirement 3**: Save orders to database  

---

## How to Run

### Step 1: Prepare the Database

**Option A - Using SQL Script (Recommended):**
1. Open SQL Server Management Studio (SSMS)
2. Open file: `Lucrarea1PSSC\database\ECommerceDatabaseSchema.sql`
3. Press F5 to execute
4. Database `ECommerceDB` is created with tables and sample data

**Option B - Automatic Creation:**
- Application will create database automatically on first run

### Step 2: Configure Connection String

**File**: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ECommerceDB;Trusted_Connection=true;Encrypt=false;MultipleActiveResultSets=true"
  }
}
```

Adjust if needed:
- `Server=.` ? Local SQL Server instance
- Change to `Server=(localdb)\\mssqllocaldb` for LocalDB
- Change to `Server=SERVERNAME` for remote server

### Step 3: Run the Application

```bash
cd Lucrarea1PSSC
dotnet run
```

---

## What Happens On Startup

```
[DATABASE] Initializing database...
[DATABASE] Database initialized successfully!

[LOAD] Loading state from database...
[DB] Loaded 10 products from database
[DB] Loaded 5 customers from database

[EVENTBUS] Setting up event subscriptions...
[EVENTBUS] Event subscriptions configured!

=== E-Commerce Shopping System ===
Event-Driven Architecture with DDD
Database-Integrated Workflow
```

---

## How Each Requirement Works

### 1?? Load State (On Startup)

```csharp
// Automatically happens in Program.cs
var dbService = serviceProvider.GetRequiredService<OrderWorkflowDatabaseService>();
List<Produs> produse = await dbService.LoadProductsFromDatabaseAsync();
List<Persoana> persoane = await dbService.LoadCustomersFromDatabaseAsync();
```

**Result**: All products and customers from database are available in memory

---

### 2?? Verify Products/Stock (Menu Option 2 - Add to Cart)

When user tries to add a product:

```
? Product exists check: Query database
  ? Found or ? Not found

? Stock check: Query database
  ? Stock >= 1 or ? Stock = 0
  
? Decision: Add to cart or show error
```

**Console Output:**
```
[DB] Product found: Laptop Dell XPS 15 (Code: 1001, Stock: 10)
[DB] Stock available for product 1001: 10 units
```

---

### 3?? Save Orders (Menu Option 10 - Place Order)

When user completes order:

```
? Validate cart (not empty, paid)
? Create order aggregate
? Save to database:
   • INSERT order into dbo.Orders
   • INSERT items into dbo.OrderItems
   • UPDATE products (decrease stock)
   • UPDATE customers (stats)
? Show confirmation
```

**Console Output:**
```
[SAVE] Saving order to database...
[DB] Order {orderNumber} created with ID {dbOrder.Id}
[DB] Order {orderNumber} successfully saved to database
[DATABASE] Order saved successfully!
```

---

## Database Tables

### Products Table
- Stores all products
- Tracks stock levels
- Used for verification when adding to cart

### Customers Table
- Stores all customers
- Tracks orders, spending, loyalty points
- Updated when orders are placed

### Orders Table
- Stores each order
- Links to customer
- Stores totals, dates, status

### OrderItems Table
- Stores line items for each order
- Links to product
- Stores quantity and price per item

---

## Sample Data

**10 Products:**
```
1001 - Laptop Dell XPS 15 (5499.99 RON, Stock: 10)
1002 - Mouse Logitech MX Master (349.99 RON, Stock: 50)
1003 - Keyboard Mechanical RGB (599.99 RON, Stock: 30)
... and 7 more
```

**5 Customers:**
```
Ion Popescu - ion.popescu@example.com
Maria Ionescu - maria.ionescu@example.com
Andrei Stanciu - andrei.stanciu@example.com
Elena Radu - elena.radu@example.com
Mihai Popa - mihai.popa@example.com
```

---

## Test Scenarios

### Test 1: Product Not Found
```
Menu: 2 (Add to cart)
Input: "Non-existent Product"
Expected: "Produsul ... nu exista in baza de date"
Status: ? Product NOT added
```

### Test 2: Out of Stock
```
Menu: 2 (Add to cart)
Input: Add product with 0 stock (deplete first if needed)
Expected: "Stoc insuficient... Disponibil: 0"
Status: ? Product NOT added
```

### Test 3: Successful Add
```
Menu: 2 (Add to cart)
Input: "Laptop Dell XPS 15"
Expected: "Produs adaugat cu succes"
Status: ? Product added to cart
```

### Test 4: Complete Order
```
Menu: 1 (Create cart for "Ion Popescu")
Menu: 2 (Add product)
Menu: 9 (Pay)
Menu: 10 (Place order for "Ion Popescu")
Expected: "[DATABASE] Order saved successfully!"
Status: ? Order in database, stock decreased
```

---

## Verify in Database

After placing an order, check the database:

```sql
-- View all orders
SELECT * FROM ECommerceDB.dbo.Orders;

-- View order items
SELECT * FROM ECommerceDB.dbo.OrderItems;

-- Check updated customer
SELECT TotalOrders, TotalSpent, LoyaltyPoints 
FROM ECommerceDB.dbo.Customers 
WHERE Name = 'Ion Popescu';

-- Check updated stock
SELECT Code, Name, QuantityInStock 
FROM ECommerceDB.dbo.Products 
WHERE Code = 1001;

-- View order summary
SELECT * FROM ECommerceDB.dbo.vw_OrderSummary;
```

---

## Troubleshooting

### Issue: Connection String Error
**Solution**: Check `appsettings.json` connection string is correct for your SQL Server instance

### Issue: Database Not Found
**Solution**: 
1. Run the SQL script manually to create database
2. OR delete database and let app recreate it

### Issue: Table Not Found
**Solution**: 
1. Database exists but not initialized
2. Run the SQL schema script
3. OR delete and let app recreate

### Issue: Authentication Failed
**Solution**: 
1. Check SQL Server is running
2. Check user has database access
3. Use appropriate connection string

---

## Files to Know

| File | Purpose |
|------|---------|
| `Program.cs` | Main app with DB integration |
| `appsettings.json` | Connection string configuration |
| `clase\Infrastructure\Database\OrderWorkflowDatabaseService.cs` | High-level DB operations |
| `clase\Infrastructure\Database\ECommerceDbContext.cs` | EF Core configuration |
| `database\ECommerceDatabaseSchema.sql` | SQL schema and sample data |

---

## Key Features

? Automatic database initialization  
? Sample data seeding  
? Product existence verification  
? Stock availability checking  
? Complete order persistence  
? Automatic stock management  
? Customer statistics tracking  
? Transaction support  
? Error handling  
? Async/await throughout  

---

## Summary

The application now provides a **complete database-integrated workflow**:

1. **On Startup**: Loads products and customers from database
2. **On Product Add**: Verifies product exists and has stock in database
3. **On Order**: Saves complete order to database with all items and stock updates

**Status**: ? Ready for Production
