-- E-Commerce Database Schema for DDD Application
-- Generated for .NET 9 / EF Core integration

USE master;
GO

-- Create database if not exists
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ECommerceDB')
BEGIN
    CREATE DATABASE ECommerceDB;
END
GO

USE ECommerceDB;
GO

-- Drop tables if they exist (for clean setup)
IF OBJECT_ID('dbo.OrderItems', 'U') IS NOT NULL DROP TABLE dbo.OrderItems;
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL DROP TABLE dbo.Products;
GO

-- Products Table
CREATE TABLE dbo.Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code INT NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL CHECK (Price >= 0),
    QuantityInStock DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (QuantityInStock >= 0),
    QuantityType NVARCHAR(20) NOT NULL DEFAULT 'Unit', -- 'Unit', 'Kilogram', etc.
    KilogramQuantity DECIMAL(18,3) NOT NULL DEFAULT 1.0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_Products_Code (Code),
    INDEX IX_Products_Name (Name)
);
GO

-- Customers Table
CREATE TABLE dbo.Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(200) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Address NVARCHAR(500) NOT NULL,
    City NVARCHAR(100),
    PostalCode NVARCHAR(20),
    Country NVARCHAR(100) DEFAULT 'Romania',
    Phone NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    TotalOrders INT NOT NULL DEFAULT 0,
    TotalSpent DECIMAL(18,2) NOT NULL DEFAULT 0,
    LoyaltyPoints INT NOT NULL DEFAULT 0,
    RegisteredAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastPurchaseAt DATETIME2,
    
    INDEX IX_Customers_Code (Code),
    INDEX IX_Customers_Email (Email),
    INDEX IX_Customers_Name (Name)
);
GO

-- Orders Table
CREATE TABLE dbo.Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderNumber UNIQUEIDENTIFIER NOT NULL UNIQUE DEFAULT NEWID(),
    OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DeliveryAddress NVARCHAR(500) NOT NULL,
    DeliveryCity NVARCHAR(100),
    DeliveryPostalCode NVARCHAR(20),
    DeliveryCountry NVARCHAR(100) DEFAULT 'Romania',
    CustomerId INT NOT NULL,
    
    Subtotal DECIMAL(18,2) NOT NULL CHECK (Subtotal >= 0),
    ShippingCost DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (ShippingCost >= 0),
    Tax DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Tax >= 0),
    Total DECIMAL(18,2) NOT NULL CHECK (Total >= 0),
    
    Status NVARCHAR(50) NOT NULL DEFAULT 'Placed', -- Placed, Shipped, Delivered, Cancelled
    PaymentMethod NVARCHAR(50),
    TransactionId NVARCHAR(100),
    
    PlacedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ShippedAt DATETIME2,
    DeliveredAt DATETIME2,
    CancelledAt DATETIME2,
    
    TrackingNumber NVARCHAR(100),
    Carrier NVARCHAR(100),
    
    Notes NVARCHAR(MAX),
    
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId) 
        REFERENCES dbo.Customers(Id) ON DELETE CASCADE,
    
    INDEX IX_Orders_OrderNumber (OrderNumber),
    INDEX IX_Orders_CustomerId (CustomerId),
    INDEX IX_Orders_OrderDate (OrderDate),
    INDEX IX_Orders_Status (Status)
);
GO

-- OrderItems Table
CREATE TABLE dbo.OrderItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductCode INT NOT NULL,
    ProductName NVARCHAR(100) NOT NULL,
    
    Quantity DECIMAL(18,2) NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
    LineTotal DECIMAL(18,2) NOT NULL CHECK (LineTotal >= 0),
    
    QuantityType NVARCHAR(20) NOT NULL DEFAULT 'Unit',
    
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) 
        REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) 
        REFERENCES dbo.Products(Id),
    
    INDEX IX_OrderItems_OrderId (OrderId),
    INDEX IX_OrderItems_ProductId (ProductId)
);
GO

-- Create computed column check constraint for Order Total
ALTER TABLE dbo.Orders ADD CONSTRAINT CK_Orders_Total 
    CHECK (Total = Subtotal + ShippingCost + Tax);
GO

-- Create trigger to update UpdatedAt on Products
CREATE TRIGGER TR_Products_UpdatedAt
ON dbo.Products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Products
    SET UpdatedAt = GETUTCDATE()
    FROM dbo.Products p
    INNER JOIN inserted i ON p.Id = i.Id;
END;
GO

-- Create trigger to update Customer statistics after order
CREATE TRIGGER TR_Orders_UpdateCustomerStats
ON dbo.Orders
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE c
    SET 
        TotalOrders = TotalOrders + 1,
        TotalSpent = TotalSpent + i.Total,
        LastPurchaseAt = i.OrderDate,
        LoyaltyPoints = LoyaltyPoints + CAST(i.Total / 10 AS INT) -- 1 point per 10 RON
    FROM dbo.Customers c
    INNER JOIN inserted i ON c.Id = i.CustomerId;
END;
GO

-- Insert sample products
INSERT INTO dbo.Products (Code, Name, Price, QuantityInStock, QuantityType, KilogramQuantity)
VALUES 
    (1001, 'Laptop Dell XPS 15', 5499.99, 10, 'Unit', 1.0),
    (1002, 'Mouse Logitech MX Master', 349.99, 50, 'Unit', 1.0),
    (1003, 'Keyboard Mechanical RGB', 599.99, 30, 'Unit', 1.0),
    (1004, 'Monitor LG 27" 4K', 1899.99, 15, 'Unit', 1.0),
    (1005, 'Laptop Lenovo ThinkPad', 4299.99, 8, 'Unit', 1.0),
    (1006, 'Webcam Logitech C920', 449.99, 25, 'Unit', 1.0),
    (1007, 'Headset HyperX Cloud II', 499.99, 40, 'Unit', 1.0),
    (1008, 'SSD Samsung 1TB', 699.99, 60, 'Unit', 1.0),
    (1009, 'RAM Corsair 16GB DDR4', 399.99, 45, 'Unit', 1.0),
    (1010, 'GPU RTX 4070', 3999.99, 5, 'Unit', 1.0);
GO

-- Insert sample customers
INSERT INTO dbo.Customers (Code, Name, Email, Address, City, PostalCode, Phone)
VALUES 
    ('CUST001', 'Ion Popescu', 'ion.popescu@example.com', 'Str. Mihai Eminescu 15', 'Cluj-Napoca', '400347', '0721234567'),
    ('CUST002', 'Maria Ionescu', 'maria.ionescu@example.com', 'Bulevardul Unirii 1', 'Bucure?ti', '030823', '0732345678'),
    ('CUST003', 'Andrei Stanciu', 'andrei.stanciu@example.com', 'Str. Avram Iancu 25', 'Timi?oara', '300088', '0743456789'),
    ('CUST004', 'Elena Radu', 'elena.radu@example.com', 'Str. Republicii 45', 'Bra?ov', '500030', '0754567890'),
    ('CUST005', 'Mihai Popa', 'mihai.popa@example.com', 'Str. Victoriei 10', 'Ia?i', '700028', '0765678901');
GO

-- Create stored procedure to check product availability
CREATE PROCEDURE sp_CheckProductAvailability
    @ProductCode INT,
    @RequiredQuantity DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        Code,
        Name,
        Price,
        QuantityInStock,
        QuantityType,
        CASE 
            WHEN QuantityInStock >= @RequiredQuantity THEN 1
            ELSE 0
        END AS IsAvailable,
        QuantityInStock - @RequiredQuantity AS RemainingStock
    FROM dbo.Products
    WHERE Code = @ProductCode AND IsActive = 1;
END;
GO

-- Create stored procedure to decrease product stock
CREATE PROCEDURE sp_DecreaseProductStock
    @ProductCode INT,
    @Quantity DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Check if product exists and has sufficient stock
        DECLARE @CurrentStock DECIMAL(18,2);
        
        SELECT @CurrentStock = QuantityInStock
        FROM dbo.Products WITH (UPDLOCK)
        WHERE Code = @ProductCode AND IsActive = 1;
        
        IF @CurrentStock IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Product not found', 16, 1);
            RETURN -1;
        END
        
        IF @CurrentStock < @Quantity
        BEGIN
            ROLLBACK TRANSACTION;
            RAISERROR('Insufficient stock', 16, 1);
            RETURN -2;
        END
        
        -- Update stock
        UPDATE dbo.Products
        SET QuantityInStock = QuantityInStock - @Quantity
        WHERE Code = @ProductCode;
        
        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Create stored procedure to place order
CREATE PROCEDURE sp_PlaceOrder
    @OrderNumber UNIQUEIDENTIFIER,
    @CustomerId INT,
    @DeliveryAddress NVARCHAR(500),
    @DeliveryCity NVARCHAR(100),
    @DeliveryPostalCode NVARCHAR(20),
    @Subtotal DECIMAL(18,2),
    @ShippingCost DECIMAL(18,2),
    @Tax DECIMAL(18,2),
    @Total DECIMAL(18,2),
    @PaymentMethod NVARCHAR(50),
    @TransactionId NVARCHAR(100),
    @OrderId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Insert order
        INSERT INTO dbo.Orders (
            OrderNumber, CustomerId, DeliveryAddress, DeliveryCity, 
            DeliveryPostalCode, Subtotal, ShippingCost, Tax, Total,
            PaymentMethod, TransactionId, Status, PlacedAt
        )
        VALUES (
            @OrderNumber, @CustomerId, @DeliveryAddress, @DeliveryCity,
            @DeliveryPostalCode, @Subtotal, @ShippingCost, @Tax, @Total,
            @PaymentMethod, @TransactionId, 'Placed', GETUTCDATE()
        );
        
        SET @OrderId = SCOPE_IDENTITY();
        
        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- Create view for order summary
CREATE VIEW vw_OrderSummary AS
SELECT 
    o.Id AS OrderId,
    o.OrderNumber,
    o.OrderDate,
    c.Name AS CustomerName,
    c.Email AS CustomerEmail,
    o.DeliveryAddress,
    o.Total,
    o.Status,
    COUNT(oi.Id) AS ItemCount,
    SUM(oi.Quantity) AS TotalQuantity
FROM dbo.Orders o
INNER JOIN dbo.Customers c ON o.CustomerId = c.Id
LEFT JOIN dbo.OrderItems oi ON o.Id = oi.OrderId
GROUP BY 
    o.Id, o.OrderNumber, o.OrderDate, c.Name, c.Email,
    o.DeliveryAddress, o.Total, o.Status;
GO

PRINT 'Database schema created successfully!';
PRINT 'Sample data inserted.';
GO
