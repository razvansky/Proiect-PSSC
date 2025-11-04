# Database Integration Implementation Guide

## Overview

This document describes the complete implementation of database integration for the e-commerce DDD application, fulfilling all three requirements:

1. ? **Load state from database** before executing workflow
2. ? **Verify product existence and stock** from database
3. ? **Save order results** to database after workflow execution

---

## Architecture Overview

```
???????????????????????????????????????????????????????????????
?                      Program.cs (Main)                      ?
?  - Initialize Database Context                              ?
?  - Load Products & Customers from Database                  ?
?  - Run Application Loop                                      ?
???????????????????????????????????????????????????????????????
                 ?
                 ??? OrderWorkflowDatabaseService
                 ?   ?? LoadProductsFromDatabaseAsync()
                 ?   ?? LoadCustomersFromDatabaseAsync()
                 ?   ?? VerifyProductExistsAsync()
                 ?   ?? CheckProductStockAsync()
                 ?   ?? PlaceOrderInDatabaseAsync()
                 ?
                 ??? UnitOfWork
                 ?   ?? ProductRepository
                 ?   ?? CustomerRepository
                 ?   ?? OrderRepository
                 ?
                 ??? ECommerceDbContext (EF Core)
                     ??? SQL Server Database
```

---

## Requirement 1: Load State from Database

### Implementation

**Location**: `Program.cs` - Lines 27-31

```csharp
// Get database service
var dbService = serviceProvider.GetRequiredService<OrderWorkflowDatabaseService>();

// Load state from database before executing workflow
List<Produs> produse = await dbService.LoadProductsFromDatabaseAsync();
List<Persoana> persoane = await dbService.LoadCustomersFromDatabaseAsync();
```

### How It Works

1. **Dependency Injection Setup**
   ```csharp
   services.AddECommerceDatabase(configuration.GetConnectionString("DefaultConnection")!);
   ```
   - Registers `ECommerceDbContext`
   - Registers `UnitOfWork`
   - Registers `OrderWorkflowDatabaseService`

2. **Database Initialization**
   ```csharp
   await serviceProvider.InitializeDatabaseAsync();
   await serviceProvider.SeedDatabaseAsync();
   ```
   - Creates database if it doesn't exist
   - Seeds initial data if empty

3. **Load Products**
   - Query: `SELECT * FROM dbo.Products WHERE IsActive = 1`
   - Converts DB models to domain `Produs` objects
   - Returns list of all available products

4. **Load Customers**
   - Query: `SELECT * FROM dbo.Customers WHERE IsActive = 1`
   - Converts DB models to domain `Persoana` objects
   - Returns list of all registered customers

### Code Location

**File**: `clase\Infrastructure\Database\OrderWorkflowDatabaseService.cs`

```csharp
public async Task<List<Produs>> LoadProductsFromDatabaseAsync()
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
```

---

## Requirement 2: Verify Product Existence and Stock from Database

### Implementation

**Location**: `Program.cs` - Case 2 (Lines 108-123)

```csharp
case 2:
    // ADD PRODUCT WITH COMMANDS AND EVENTS
    Console.WriteLine("Introduceti numele produsului:");
    numeProdus = Console.ReadLine();

    // ? REQUIREMENT 2: Verify product existence and stock from database
    var (productExists, dbProduct) = await dbService.VerifyProductExistsAsync(addCommand.NumeProdus);
    if (!productExists)
    {
        Console.WriteLine($"Eroare: Produsul {addCommand.NumeProdus} nu exista in baza de date");
        break;
    }

    var (hasStock, availableStock) = await dbService.CheckProductStockAsync(dbProduct!.Code, 1);
    if (!hasStock)
    {
        Console.WriteLine($"Eroare: Stoc insuficient pentru {addCommand.NumeProdus}. Disponibil: {availableStock}");
        break;
    }

    // Add to cart...
```

### How It Works

#### A. Verify Product Existence

**File**: `clase\Infrastructure\Database\OrderWorkflowDatabaseService.cs`

```csharp
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
```

**Database Query**:
```sql
SELECT * FROM dbo.Products 
WHERE Name = @productName AND IsActive = 1
```

#### B. Check Product Stock

**File**: `clase\Infrastructure\Database\OrderWorkflowDatabaseService.cs`

```csharp
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
```

**Database Query**:
```sql
SELECT QuantityInStock FROM dbo.Products 
WHERE Code = @productCode AND IsActive = 1
```

### Verification Flow

```
User Input: Product Name
    ?
VerifyProductExistsAsync()
    ?? Query database by name
    ?? Return product if found
    ?? Return (false, null) if not found
    ?
CheckProductStockAsync()
    ?? Get product code
    ?? Query database for stock level
    ?? Compare with required quantity
    ?? Return (hasStock, availableQuantity)
    ?
Decision: Add to cart or show error
```

---

## Requirement 3: Save Order Result to Database

### Implementation

**Location**: `Program.cs` - Case 10 (Lines 248-290)

```csharp
case 10:
    // PLACE ORDER WITH DATABASE PERSISTENCE
    // ... validation code ...

    // ? REQUIREMENT 3: Save order result to database
    Console.WriteLine("[SAVE] Saving order to database...");
    var (orderSuccess, order, orderError) = 
        ComandaAggregate.CreateFromPaidCart(
            persoanaComanda, 
            persoanaComanda.CosCurent
        );

    if (orderSuccess)
    {
        var successEvent = order.ToSuccessEvent();
        
        // Save to database
        var (dbSaveSuccess, dbOrder, dbSaveError) = await dbService.PlaceOrderInDatabaseAsync(
            order.ComandaId,
            persoanaComanda,
            persoanaComanda.CosCurent,
            produse
        );

        if (dbSaveSuccess)
        {
            Console.WriteLine(successEvent.Message);
            Console.WriteLine($"[DATABASE] Order saved successfully!\n");
            EventBus.Publish(successEvent);
        }
        else
        {
            Console.WriteLine($"[DATABASE ERROR] Failed to save order: {dbSaveError}");
        }
    }
    break;
```

### How It Works

**File**: `clase\Infrastructure\Database\OrderWorkflowDatabaseService.cs`

```csharp
public async Task<(bool Success, OrderDbModel? Order, string? Error)> PlaceOrderInDatabaseAsync(
    Guid orderNumber,
    Persoana customer,
    CosDeCumparaturi cart,
    List<Produs> products)
{
    try
    {
        // 1. Get customer from database
        var dbCustomer = await _unitOfWork.Customers.GetCustomerByNameAsync(customer.Nume.Name);
        if (dbCustomer == null)
            return (false, null, $"Customer {customer.Nume.Name} not found in database");

        // 2. Get cart items
        var cartItems = cart.GetProduseCos();
        if (cartItems == null || cartItems.Count == 0)
            return (false, null, "Cart is empty");

        // 3. Calculate totals
        decimal subtotal = (decimal)cart.TotalCos();
        decimal shippingCost = subtotal > 500 ? 0 : 20;
        decimal tax = subtotal * 0.19m;

        // 4. Create order in database
        var dbOrder = await _unitOfWork.Orders.CreateOrderAsync(
            orderNumber,
            dbCustomer.Id,
            customer.Adress.adress,
            null,
            null,
            subtotal,
            shippingCost,
            tax,
            "Manual",
            orderNumber.ToString()
        );

        // 5. Add order items and decrease stock
        foreach (var cartItem in cartItems)
        {
            var dbProduct = await _unitOfWork.Products.GetProductByNameAsync(cartItem.Nume);
            if (dbProduct == null)
                return (false, null, $"Product {cartItem.Nume} not found");

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
                return (false, null, $"Failed to decrease stock for {cartItem.Nume}");
        }

        // 6. Update customer loyalty points
        await _unitOfWork.Customers.UpdateLoyaltyPointsAsync(
            dbCustomer.Id,
            (int)(subtotal / 10)
        );

        // 7. Save all changes
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"[DB] Order {orderNumber} successfully saved");
        return (true, dbOrder, null);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB ERROR] Failed to place order: {ex.Message}");
        return (false, null, $"Database error: {ex.Message}");
    }
}
```

### Order Placement Flow

```
Workflow Validation
    ?
Create Order Aggregate
    ?
Call PlaceOrderInDatabaseAsync()
    ?? Get Customer from DB
    ?? Validate Cart Items
    ?? Calculate Totals (Subtotal, Shipping, Tax)
    ?? Create Order in DB
    ?? Add Order Items in DB
    ?? Decrease Product Stock in DB
    ?? Update Customer Loyalty Points
    ?? Commit Transaction
    ?
Publish Success Event
    ?
Display Order Confirmation
```

### Database Operations

**1. Create Order**
```sql
INSERT INTO dbo.Orders (
    OrderNumber, CustomerId, DeliveryAddress, 
    Subtotal, ShippingCost, Tax, Total,
    PaymentMethod, TransactionId, Status, PlacedAt
) VALUES (...)
```

**2. Add Order Items**
```sql
INSERT INTO dbo.OrderItems (
    OrderId, ProductId, ProductCode, ProductName,
    Quantity, UnitPrice, LineTotal, QuantityType
) VALUES (...)
```

**3. Decrease Stock** (via stored procedure)
```sql
EXEC sp_DecreaseProductStock @ProductCode, @Quantity
```

**4. Update Customer**
```sql
UPDATE dbo.Customers SET
    TotalOrders = TotalOrders + 1,
    TotalSpent = TotalSpent + @Total,
    LastPurchaseAt = @OrderDate,
    LoyaltyPoints = LoyaltyPoints + @Points
WHERE Id = @CustomerId
```

---

## Data Flow Diagram

```
???????????????????????????????????????????????????????????
?              Application Startup                         ?
???????????????????????????????????????????????????????????
? 1. Initialize EF Core DbContext                         ?
? 2. Create Database (if needed)                          ?
? 3. Seed Sample Data (if empty)                          ?
? 4. Load Products from Database ? List<Produs>          ?
? 5. Load Customers from Database ? List<Persoana>       ?
???????????????????????????????????????????????????????????
                 ?
                 ?
???????????????????????????????????????????????????????????
?           User Adds Product to Cart                      ?
???????????????????????????????????????????????????????????
? 1. User Input: Product Name                             ?
? 2. VerifyProductExistsAsync()                           ?
?    ? Query: SELECT * FROM Products WHERE Name = ?       ?
?    ? Return: Product found or Not Found                 ?
? 3. CheckProductStockAsync()                             ?
?    ? Query: SELECT QuantityInStock FROM Products        ?
?    ? Compare: Available >= Required                     ?
? 4. Decision: Add to Cart OR Show Error                 ?
? 5. Update In-Memory Product List                        ?
???????????????????????????????????????????????????????????
                 ?
                 ?
???????????????????????????????????????????????????????????
?           User Places Order                              ?
???????????????????????????????????????????????????????????
? 1. Validate Cart (not empty, paid, valid)              ?
? 2. Create Order Aggregate                               ?
? 3. PlaceOrderInDatabaseAsync()                          ?
?    ?? INSERT INTO Orders                                ?
?    ?? INSERT INTO OrderItems (x N)                      ?
?    ?? UPDATE Products (decrease stock)                  ?
?    ?? UPDATE Customers (statistics)                     ?
?    ?? COMMIT TRANSACTION                                ?
? 4. Publish Success Event                                ?
? 5. Display Confirmation                                 ?
???????????????????????????????????????????????????????????
```

---

## Database Schema Integration

### Products Table
- **Used for**: Stock verification, price checking
- **Queries**: SELECT by name, code, or all
- **Updates**: Stock decrease, update timestamps

### Customers Table
- **Used for**: Customer lookup, loyalty tracking
- **Queries**: SELECT by name, code, or email
- **Updates**: Order count, total spent, loyalty points

### Orders Table
- **Used for**: Order creation, order history
- **Queries**: SELECT by order number, customer
- **Inserts**: New orders

### OrderItems Table
- **Used for**: Line items, product tracking
- **Queries**: SELECT items per order
- **Inserts**: New line items

---

## Error Handling

### Database Connection Errors
```
[DB ERROR] Failed to connect to database
? Application falls back to in-memory mode
? User sees error message
? No orders are saved
```

### Product Not Found
```
User enters: "Non-existent Product"
? VerifyProductExistsAsync() returns (false, null)
? Message: "Produsul ... nu exista in baza de date"
? Product NOT added to cart
```

### Insufficient Stock
```
User adds: Product with 1 unit available, needs 2
? CheckProductStockAsync() returns (false, 1)
? Message: "Stoc insuficient... Disponibil: 1"
? Product NOT added to cart
```

### Order Save Failure
```
Transaction fails during order save
? PlaceOrderInDatabaseAsync() catches exception
? Returns (false, null, error message)
? Message: "[DATABASE ERROR] Failed to save order: ..."
? Order is NOT created
? Cart remains paid (not cleared automatically)
```

---

## Configuration

### Connection String
**File**: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ECommerceDB;Trusted_Connection=true;Encrypt=false;MultipleActiveResultSets=true"
  }
}
```

### Database Initialization
**File**: `clase\Infrastructure\Database\DatabaseConfiguration.cs`

```csharp
services.AddECommerceDatabase(
    configuration.GetConnectionString("DefaultConnection")!
);
```

---

## Testing the Implementation

### Test 1: Load State
```
Expected: Products and Customers loaded from database
Action: Start application
Result: Console output shows "[DB] Loaded X products from database"
```

### Test 2: Verify Product
```
Expected: Product verification queries database
Action: Try to add existing and non-existing products
Result: Existing products added, non-existing rejected with DB message
```

### Test 3: Check Stock
```
Expected: Stock verification queries database
Action: Add product until stock runs out
Result: "Stoc insuficient" message when stock = 0
```

### Test 4: Save Order
```
Expected: Order saved to database with all items
Action: Complete order workflow
Result: "[DATABASE] Order saved successfully!" message
Result: Order visible in database query
```

---

## Summary

? **All Three Requirements Implemented**:

1. **Load State**: Products & Customers loaded from database on startup
2. **Verify Products & Stock**: Database queries verify existence and availability
3. **Save Results**: Orders completely saved to database with all items and stock updates

? **Features**:
- Automatic stock management
- Customer statistics tracking
- Order history persistence
- Transactional integrity
- Error handling and logging

? **Production Ready**:
- Type-safe database access
- Async/await throughout
- Proper transaction handling
- Comprehensive error messages
