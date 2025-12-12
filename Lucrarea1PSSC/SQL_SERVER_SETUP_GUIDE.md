# SQL Server Setup Guide for Lucrarea1PSSC

## Problem Analysis

Your application was throwing this error:
```
Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server. 
The server was not found or was not accessible. 
(provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)
```

### Root Causes:
1. **Invalid appsettings.json path**: Used UNC network path (`@"\\resources\\appsettings.json"`) instead of relative path
2. **SQL Server not running or not accessible** on `Server=.` (default instance)
3. **Named Pipes protocol** not enabled or SQL Server not installed

---

## Solution: Two Options

### Option 1: Use LocalDB (Recommended for Development) ?

**What is LocalDB?**
- Lightweight SQL Server Express designed for development
- Automatically installed with Visual Studio
- No manual startup required
- Integrated authentication

**Already configured!** Your `appsettings.json` now uses:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ECommerceDB;Integrated Security=true;Encrypt=false;MultipleActiveResultSets=true;"
```

**To verify LocalDB is working:**

1. Open Visual Studio Developer Command Prompt
2. Run:
   ```powershell
   sqllocaldb info mssqllocaldb
   ```
3. If not running, start it:
   ```powershell
   sqllocaldb start mssqllocaldb
   ```

### Option 2: Use Full SQL Server (For Production/Advanced)

If you want to use a full SQL Server instance instead:

**Installation:**

1. **Download SQL Server Express** (free tier)
   - Go to: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Download "SQL Server 2022 Express"

2. **Install SQL Server**
   - Run installer
   - Choose "Custom" installation
   - Select "Database Engine Services"
   - Accept default instance name or use a named instance
   - Choose "Mixed Mode" or "Windows Authentication"
   - Note your instance name

3. **Update appsettings.json** with your server details:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=ECommerceDB;Trusted_Connection=true;Encrypt=false;MultipleActiveResultSets=true;"
     }
   }
   ```

   Examples:
   - **Default instance**: `Server=.` or `Server=COMPUTERNAME`
   - **Named instance**: `Server=COMPUTERNAME\\SQLEXPRESS` or `Server=.\SQLEXPRESS`
   - **Remote server**: `Server=192.168.1.100;User Id=sa;Password=YourPassword;`

4. **Verify SQL Server is running:**
   - Windows: Press `Win+R`, type `services.msc`, find "SQL Server (MSSQLSERVER)" or your instance name, ensure it's "Running"
   - Command line:
     ```powershell
     Get-Service MSSQLSERVER
     ```

---

## Connection String Options

### LocalDB (Development) - Current Setting
```
Server=(localdb)\mssqllocaldb;Database=ECommerceDB;Integrated Security=true;Encrypt=false;MultipleActiveResultSets=true;
```

### SQL Server Default Instance (Windows Auth)
```
Server=.;Database=ECommerceDB;Trusted_Connection=true;Encrypt=false;MultipleActiveResultSets=true;
```

### SQL Server Named Instance (Windows Auth)
```
Server=.\SQLEXPRESS;Database=ECommerceDB;Trusted_Connection=true;Encrypt=false;MultipleActiveResultSets=true;
```

### SQL Server with SQL Authentication
```
Server=YOUR_SERVER;Database=ECommerceDB;User Id=sa;Password=YourPassword;Encrypt=false;MultipleActiveResultSets=true;
```

### Remote SQL Server
```
Server=192.168.1.100;Database=ECommerceDB;User Id=sa;Password=YourPassword;Encrypt=true;TrustServerCertificate=true;MultipleActiveResultSets=true;
```

---

## Database Creation Flow

When your application starts:

1. **Program.cs** loads `appsettings.json` from the application directory
2. **DatabaseConfiguration.InitializeDatabaseAsync()** is called
3. **ECommerceDbContext.Database.EnsureCreatedAsync()** creates:
   - `ECommerceDB` database (if it doesn't exist)
   - Four tables:
     - `Products` - Inventory items
     - `Customers` - Customer records
     - `Orders` - Order headers
     - `OrderItems` - Order line items
4. **SeedDatabaseAsync()** populates initial test data:
   - 10 products (laptops, peripherals, components)
   - 5 customers (sample data)

---

## Troubleshooting

### Error: "Could not open a connection to SQL Server"

**Solution 1: Verify LocalDB is running**
```powershell
sqllocaldb start mssqllocaldb
```

**Solution 2: Check SQL Server service is running**
```powershell
Get-Service MSSQLSERVER  # For default instance
Get-Service MSSQL$SQLEXPRESS  # For named instance
```

**Solution 3: Verify connection string in appsettings.json**
- Ensure file is in application output directory (bin/Debug/net9.0/)
- Check file is not corrupted (valid JSON)
- Verify `DefaultConnection` key exists

**Solution 4: Enable Named Pipes protocol (if using TCP/Named Pipes)**
1. Open SQL Server Configuration Manager
2. Navigate to SQL Server Network Configuration ? Protocols for [Instance Name]
3. Enable "Named Pipes"
4. Restart SQL Server service

### Error: "Connection string not found"

**Solution:**
- Ensure `appsettings.json` exists in `AppContext.BaseDirectory`
- Run from the project root or ensure the file is copied to output directory
- Add to `.csproj` if needed:
  ```xml
  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
  ```

### Error: "Authentication failed"

**Solution:**
- For **Windows Authentication**: Ensure your Windows user has SQL Server access
- For **SQL Authentication**: Verify username and password in connection string
- Check if SQL Server is in Mixed Mode (SQL Auth requires this)

---

## Verify Everything Works

After configuring, run your application. You should see:

```
[DATABASE] Initializing database...
[DB] Database initialized successfully
[DB] Database seeded with initial data
[DATABASE] Database initialized successfully!

[LOAD] Loading state from database...

=== E-Commerce Shopping System ===
Event-Driven Architecture with DDD
Database-Integrated Workflow

[10 products and 5 customers loaded...]
```

---

## What Changed in Your Code

### Program.cs
- Fixed appsettings.json path from `@"\\resources\\appsettings.json"` to `"appsettings.json"`
- Added validation for null/empty connection strings
- Added debug output to show which connection string is being used
- Enhanced error handling with more informative messages

### appsettings.json
- Updated connection string to use LocalDB: `(localdb)\mssqllocaldb`
- LocalDB is included with Visual Studio and requires no setup

---

## Next Steps

1. **Run your application** - it should now connect to LocalDB automatically
2. **If you get errors**, check the troubleshooting section above
3. **To use a different SQL Server**, update the connection string in `appsettings.json` and restart the app
4. **For production**, migrate to a full SQL Server instance with proper backup and security configuration

---

## Database Schema

Your application creates these tables automatically:

### Products
- Id (Primary Key)
- Code (Unique)
- Name (100 chars max)
- Price (Decimal 18,2)
- QuantityInStock (Decimal 18,2)
- QuantityType (e.g., "Unit")
- IsActive
- CreatedAt, UpdatedAt

### Customers
- Id (Primary Key)
- Code (Unique)
- Name (200 chars max)
- Email (Unique)
- Address (500 chars max)
- City, PostalCode, Country
- Phone
- TotalOrders, TotalSpent, LoyaltyPoints
- RegisteredAt, LastPurchaseAt

### Orders
- Id (Primary Key)
- OrderNumber (Unique GUID)
- CustomerId (Foreign Key)
- DeliveryAddress, City, PostalCode, Country
- Subtotal, ShippingCost, Tax, Total
- Status, PaymentMethod, TransactionId
- PlacedAt, ShippedAt, DeliveredAt, CancelledAt
- TrackingNumber, Carrier
- Notes

### OrderItems
- Id (Primary Key)
- OrderId (Foreign Key)
- ProductId (Foreign Key)
- ProductCode, ProductName
- Quantity, UnitPrice, LineTotal
- QuantityType

---

## Questions?

If you encounter issues:
1. Check error messages for specific SQL errors
2. Verify LocalDB/SQL Server is running
3. Ensure appsettings.json is in the output directory
4. Check connection string format for your environment
