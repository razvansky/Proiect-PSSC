# Entity States Implementation Documentation

## Overview
This document describes the entity states created following the DDD pattern from `copilot_instructions_full.md`. Each entity represents a bounded context aggregate with complete lifecycle management through immutable state transitions.

---

## 1. ShoppingCart Entity (`Lucrarea1PSSC\clase\ClaseCos\Entities\ShoppingCartEntity.cs`)

### State Flow Diagram
```
UnvalidatedShoppingCart ? ValidatedShoppingCart ? PayedShoppingCart ? CompletedShoppingCart
                       ? InvalidShoppingCart
```

### States

#### 1.1 UnvalidatedShoppingCart
**Purpose:** Initial raw input state before validation

**Properties:**
- `CustomerName: string` - Raw customer name input
- `ProductNames: IEnumerable<string>` - List of product names to add

**Usage:**
```csharp
var unvalidated = new ShoppingCartEntity.UnvalidatedShoppingCart(
    "John Doe",
    new[] { "Laptop", "Mouse", "Keyboard" }
);
```

#### 1.2 ValidatedShoppingCart
**Purpose:** Validated state with proper value objects and calculated total

**Properties:**
- `CustomerId: string` - Validated customer identifier
- `Products: IReadOnlyCollection<CartProduct>` - Validated products with quantities
- `Total: Money` - Calculated total amount
- `CreatedAt: DateTime` - Cart creation timestamp

**Invariants:**
- Products collection is read-only
- Total is calculated and immutable
- All products have valid prices

#### 1.3 PayedShoppingCart
**Purpose:** Cart after successful payment processing

**Properties:**
- All properties from `ValidatedShoppingCart`
- `PaymentMethod: PaymentMethod` - How payment was made (Cash, Card, etc.)
- `TransactionId: string` - Unique transaction identifier
- `PaymentDate: DateTime` - When payment was processed

**Invariants:**
- Payment date cannot be before creation date
- Transaction ID is unique
- Cart is now immutable

#### 1.4 CompletedShoppingCart
**Purpose:** Final state after order placement

**Properties:**
- All properties from `PayedShoppingCart`
- `OrderId: Guid` - Associated order identifier
- `CompletionDate: DateTime` - When order was placed

**Invariants:**
- Order ID links to a valid order
- Completion date is after payment date
- This is a terminal state

#### 1.5 InvalidShoppingCart
**Purpose:** Failure state with validation/processing errors

**Properties:**
- `Reasons: IEnumerable<string>` - Collection of error messages

**Usage:**
```csharp
var invalid = new ShoppingCartEntity.InvalidShoppingCart(
    new[] { 
        "Customer name cannot be empty",
        "At least one product required"
    }
);
```

### Supporting Types

#### CartProduct
Represents a product in the cart with quantity and pricing:
```csharp
public record CartProduct
{
    public int ProductCode { get; }
    public ProductName ProductName { get; }
    public int Quantity { get; }
    public Money UnitPrice { get; }
    public Money TotalPrice { get; }
}
```

#### PaymentMethod Enum
```csharp
public enum PaymentMethod
{
    Cash,
    CreditCard,
    DebitCard,
    BankTransfer,
    OnlinePayment
}
```

---

## 2. Order Entity (`Lucrarea1PSSC\clase\Workflow\Entities\OrderEntity.cs`)

### State Flow Diagram
```
UnvalidatedOrder ? ValidatedOrder ? PlacedOrder ? ShippedOrder ? DeliveredOrder
                ? InvalidOrder                  ? CancelledOrder
```

### States

#### 2.1 UnvalidatedOrder
**Purpose:** Initial raw order data from user input

**Properties:**
- `CustomerName: string` - Raw customer name
- `DeliveryAddress: string` - Raw delivery address
- `Email: string` - Raw email address
- `OrderLines: IEnumerable<UnvalidatedOrderLine>` - Raw order items

#### 2.2 ValidatedOrder
**Purpose:** Fully validated order ready for placement

**Properties:**
- `CustomerId: string` - Validated customer ID
- `CustomerEmail: EmailAddress` - Validated email value object
- `DeliveryAddress: DeliveryAddress` - Validated address value object
- `OrderLines: IReadOnlyCollection<ValidatedOrderLine>` - Validated items
- `Subtotal: Money` - Sum of line items
- `ShippingCost: Money` - Calculated shipping
- `Tax: Money` - Calculated tax
- `Total: Money` - Final total amount
- `CreatedAt: DateTime` - Order creation time

**Invariants:**
- `Total = Subtotal + ShippingCost + Tax`
- All order lines have positive quantities
- Delivery address is complete and valid

#### 2.3 PlacedOrder
**Purpose:** Order confirmed and placed in system

**Properties:**
- All properties from `ValidatedOrder`
- `OrderNumber: OrderNumber` - Unique order number value object
- `PlacedAt: DateTime` - Order placement timestamp
- `EstimatedDeliveryDate: DateTime` - Estimated delivery

**Invariants:**
- Order number is unique
- Placed date is after created date
- Estimated delivery is in the future

#### 2.4 ShippedOrder
**Purpose:** Order dispatched for delivery

**Properties:**
- Key properties from `PlacedOrder`
- `ShippedAt: DateTime` - Shipment timestamp
- `TrackingNumber: string` - Courier tracking number
- `Carrier: string` - Delivery carrier name
- `EstimatedDeliveryDate: DateTime` - Updated delivery estimate

**Invariants:**
- Shipped date is after placed date
- Tracking number is not empty
- Carrier is specified

#### 2.5 DeliveredOrder
**Purpose:** Final successful state - order delivered

**Properties:**
- Key properties from `ShippedOrder`
- `DeliveredAt: DateTime` - Actual delivery timestamp
- `RecipientName: string?` - Who received the order
- `Signature: string?` - Proof of delivery

**Invariants:**
- Delivered date is after shipped date
- This is a terminal successful state

#### 2.6 CancelledOrder
**Purpose:** Order was cancelled before or after shipment

**Properties:**
- Key properties from `PlacedOrder`
- `CancelledAt: DateTime` - Cancellation timestamp
- `CancellationReason: string` - Why order was cancelled
- `CancelledBy: string?` - Who cancelled (customer/admin)
- `RefundIssued: bool` - Whether refund was processed

**Invariants:**
- Cancelled date is after placed date
- Cancellation reason is required
- This is a terminal failure state

#### 2.7 InvalidOrder
**Purpose:** Validation or processing failed

**Properties:**
- `Reasons: IEnumerable<string>` - Collection of error messages

### Supporting Types

#### UnvalidatedOrderLine
Raw order line before validation:
```csharp
public record UnvalidatedOrderLine
{
    public string ProductName { get; }
    public int Quantity { get; }
    public double UnitPrice { get; }
}
```

#### ValidatedOrderLine
Validated order line with value objects:
```csharp
public record ValidatedOrderLine
{
    public int ProductCode { get; }
    public ProductName ProductName { get; }
    public int Quantity { get; }
    public Money UnitPrice { get; }
    public Money LineTotal { get; }
}
```

---

## 3. Customer Entity (`Lucrarea1PSSC\clase\ClaseGestionarePersoane\Entities\CustomerEntity.cs`)

### State Flow Diagram
```
UnvalidatedCustomer ? ValidatedCustomer ? ActiveCustomer ? PremiumCustomer
                   ? InvalidCustomer                      ? InactiveCustomer
```

### States

#### 3.1 UnvalidatedCustomer
**Purpose:** Initial registration data

**Properties:**
- `Name: string` - Raw name input
- `Email: string` - Raw email input
- `Address: string` - Raw address input
- `Phone: string?` - Optional phone number

#### 3.2 ValidatedCustomer
**Purpose:** Validated registration - account created

**Properties:**
- `CustomerId: string` - Unique customer identifier
- `Name: string` - Validated name
- `Email: EmailAddress` - Validated email value object
- `PrimaryAddress: DeliveryAddress` - Validated address value object
- `RegisteredAt: DateTime` - Registration timestamp

**Invariants:**
- Customer ID is unique
- Email is valid and unique
- Primary address is complete

#### 3.3 ActiveCustomer
**Purpose:** Customer with purchase history

**Properties:**
- All properties from `ValidatedCustomer`
- `SavedAddresses: IReadOnlyCollection<DeliveryAddress>` - Multiple addresses
- `LastPurchaseDate: DateTime` - Most recent purchase
- `TotalOrders: int` - Number of completed orders
- `TotalSpent: double` - Lifetime purchase value
- `LoyaltyPoints: int` - Earned loyalty points

**Invariants:**
- At least one completed order
- Total orders and spent are positive
- Last purchase date is after registration

#### 3.4 PremiumCustomer
**Purpose:** VIP customer with special benefits

**Properties:**
- All properties from `ActiveCustomer`
- `PremiumSince: DateTime` - When premium status granted
- `DiscountPercentage: double` - Special discount rate
- `PriorityShipping: bool` - Free priority shipping
- `FreeReturns: bool` - Free return shipping
- `TierName: string` - Tier level (Gold, Platinum, Diamond)

**Invariants:**
- Premium since date is after registration
- Discount percentage is between 0-100
- Tier name matches CustomerTier enum

#### 3.5 InactiveCustomer
**Purpose:** Customer with no recent activity

**Properties:**
- Key properties from `ActiveCustomer`
- `InactiveSince: DateTime` - When marked inactive
- `InactivityReason: string` - Why customer became inactive

**Invariants:**
- Inactive since is after last purchase
- At least 90 days since last purchase

#### 3.6 InvalidCustomer
**Purpose:** Validation failed

**Properties:**
- `Reasons: IEnumerable<string>` - Error messages

### Supporting Types

#### CustomerTier Enum
```csharp
public enum CustomerTier
{
    Standard,    // New customers
    Silver,      // 5+ orders or 1000 RON spent
    Gold,        // 20+ orders or 5000 RON spent
    Platinum,    // 50+ orders or 15000 RON spent
    Diamond      // 100+ orders or 50000 RON spent
}
```

---

## Design Patterns Used

### 1. State Pattern
Each entity has multiple states representing its lifecycle:
- Immutable state transitions
- Clear state progression
- Invalid state for error handling

### 2. Type Safety
- Strong typing with value objects
- Compile-time guarantees
- No primitive obsession

### 3. Immutability
- All records are immutable
- Internal constructors prevent direct instantiation
- IReadOnlyCollection for lists

### 4. Encapsulation
- Internal constructors
- Public interface for pattern matching
- Private state management

### 5. Domain Events
Each state transition can trigger domain events:
- `CartValidatedEvent`
- `PaymentProcessedEvent`
- `OrderPlacedEvent`
- `OrderShippedEvent`
- `CustomerUpgradedEvent`

---

## Usage Examples

### Example 1: Shopping Cart Flow

```csharp
// 1. Start with unvalidated cart
var unvalidatedCart = new ShoppingCartEntity.UnvalidatedShoppingCart(
    "John Doe",
    new[] { "Laptop", "Mouse" }
);

// 2. Validate cart
var validatedCart = new ShoppingCartEntity.ValidatedShoppingCart(
    "CUST001",
    new List<CartProduct> {
        new CartProduct(1, ProductName.TryParse("Laptop"), 1, Money.FromAmount(2500, "RON"), Money.FromAmount(2500, "RON"))
    }.AsReadOnly(),
    Money.FromAmount(2500, "RON"),
    DateTime.UtcNow
);

// 3. Process payment
var payedCart = new ShoppingCartEntity.PayedShoppingCart(
    validatedCart.CustomerId,
    validatedCart.Products,
    validatedCart.Total,
    validatedCart.CreatedAt,
    PaymentMethod.CreditCard,
    "TXN123456",
    DateTime.UtcNow
);

// 4. Complete with order
var completedCart = new ShoppingCartEntity.CompletedShoppingCart(
    payedCart.CustomerId,
    payedCart.Products,
    payedCart.Total,
    payedCart.CreatedAt,
    payedCart.PaymentMethod,
    payedCart.TransactionId,
    payedCart.PaymentDate,
    Guid.NewGuid(),
    DateTime.UtcNow
);
```

### Example 2: Order Lifecycle

```csharp
// 1. Unvalidated order from user
var unvalidatedOrder = new OrderEntity.UnvalidatedOrder(
    "Jane Smith",
    "Str. Libertatii 10, Cluj",
    "jane@example.com",
    new[] {
        new UnvalidatedOrderLine("Laptop", 1, 2500),
        new UnvalidatedOrderLine("Mouse", 2, 50)
    }
);

// 2. Validate and calculate
var validatedOrder = new OrderEntity.ValidatedOrder(
    "CUST002",
    EmailAddress.TryParse("jane@example.com").Address,
    DeliveryAddress.Create("Str. Libertatii 10", "Cluj", "400000"),
    validatedLines,
    Money.FromAmount(2600, "RON"),
    Money.FromAmount(20, "RON"),
    Money.FromAmount(494, "RON"),
    Money.FromAmount(3114, "RON"),
    DateTime.UtcNow
);

// 3. Place order
var placedOrder = new OrderEntity.PlacedOrder(
    OrderNumber.Create(),
    validatedOrder.CustomerId,
    validatedOrder.CustomerEmail,
    validatedOrder.DeliveryAddress,
    validatedOrder.OrderLines,
    validatedOrder.Subtotal,
    validatedOrder.ShippingCost,
    validatedOrder.Tax,
    validatedOrder.Total,
    validatedOrder.CreatedAt,
    DateTime.UtcNow,
    DateTime.UtcNow.AddDays(3)
);

// 4. Ship order
var shippedOrder = new OrderEntity.ShippedOrder(
    placedOrder.OrderNumber,
    placedOrder.CustomerId,
    placedOrder.CustomerEmail,
    placedOrder.DeliveryAddress,
    placedOrder.OrderLines,
    placedOrder.Total,
    placedOrder.CreatedAt,
    placedOrder.PlacedAt,
    DateTime.UtcNow,
    "TRACK123456",
    "Fan Courier",
    placedOrder.EstimatedDeliveryDate
);

// 5. Deliver order
var deliveredOrder = new OrderEntity.DeliveredOrder(
    shippedOrder.OrderNumber,
    shippedOrder.CustomerId,
    shippedOrder.CustomerEmail,
    shippedOrder.DeliveryAddress,
    shippedOrder.OrderLines,
    shippedOrder.Total,
    shippedOrder.CreatedAt,
    shippedOrder.PlacedAt,
    shippedOrder.ShippedAt,
    DateTime.UtcNow,
    shippedOrder.TrackingNumber,
    shippedOrder.Carrier,
    "Jane Smith",
    "JSmith2024"
);
```

### Example 3: Customer Progression

```csharp
// 1. New registration
var unvalidatedCustomer = new CustomerEntity.UnvalidatedCustomer(
    "John Doe",
    "john@example.com",
    "Str. Mihai Eminescu 15, Cluj",
    "0721234567"
);

// 2. Validate and create account
var validatedCustomer = new CustomerEntity.ValidatedCustomer(
    "CUST003",
    "John Doe",
    EmailAddress.TryParse("john@example.com").Email,
    DeliveryAddress.Create("Str. Mihai Eminescu 15", "Cluj", "400347"),
    DateTime.UtcNow
);

// 3. After first purchase
var activeCustomer = new CustomerEntity.ActiveCustomer(
    validatedCustomer.CustomerId,
    validatedCustomer.Name,
    validatedCustomer.Email,
    validatedCustomer.PrimaryAddress,
    new List<DeliveryAddress>().AsReadOnly(),
    validatedCustomer.RegisteredAt,
    DateTime.UtcNow,
    totalOrders: 1,
    totalSpent: 2500,
    loyaltyPoints: 250
);

// 4. Upgrade to premium
var premiumCustomer = new CustomerEntity.PremiumCustomer(
    activeCustomer.CustomerId,
    activeCustomer.Name,
    activeCustomer.Email,
    activeCustomer.PrimaryAddress,
    activeCustomer.SavedAddresses,
    activeCustomer.RegisteredAt,
    activeCustomer.LastPurchaseDate,
    totalOrders: 25,
    totalSpent: 10000,
    loyaltyPoints: 2000,
    DateTime.UtcNow,
    discountPercentage: 10,
    priorityShipping: true,
    freeReturns: true,
    tierName: "Gold"
);
```

---

## Pattern Matching Usage

### Shopping Cart State Handling
```csharp
public string HandleCart(ShoppingCartEntity.IShoppingCart cart) => cart switch
{
    ShoppingCartEntity.UnvalidatedShoppingCart unvalidated => 
        $"Validating cart for {unvalidated.CustomerName}",
    
    ShoppingCartEntity.ValidatedShoppingCart validated => 
        $"Cart validated: {validated.Products.Count} items, Total: {validated.Total}",
    
    ShoppingCartEntity.PayedShoppingCart payed => 
        $"Payment processed: {payed.TransactionId} via {payed.PaymentMethod}",
    
    ShoppingCartEntity.CompletedShoppingCart completed => 
        $"Order {completed.OrderId} placed successfully",
    
    ShoppingCartEntity.InvalidShoppingCart invalid => 
        $"Cart validation failed: {string.Join(", ", invalid.Reasons)}",
    
    _ => throw new InvalidOperationException("Unknown cart state")
};
```

### Order State Handling
```csharp
public string HandleOrder(OrderEntity.IOrder order) => order switch
{
    OrderEntity.UnvalidatedOrder => "Processing order...",
    OrderEntity.ValidatedOrder validated => $"Order validated: {validated.Total}",
    OrderEntity.PlacedOrder placed => $"Order {placed.OrderNumber} placed",
    OrderEntity.ShippedOrder shipped => $"Tracking: {shipped.TrackingNumber}",
    OrderEntity.DeliveredOrder delivered => $"Delivered on {delivered.DeliveredAt}",
    OrderEntity.CancelledOrder cancelled => $"Cancelled: {cancelled.CancellationReason}",
    OrderEntity.InvalidOrder invalid => $"Errors: {string.Join(", ", invalid.Reasons)}",
    _ => throw new InvalidOperationException("Unknown order state")
};
```

---

## Build Status
? **Build: SUCCESSFUL**

---

## Benefits

### 1. Type Safety
- Compiler enforces valid state transitions
- Cannot create invalid states
- Pattern matching ensures all states handled

### 2. Immutability
- Thread-safe by design
- No accidental state mutations
- Clear audit trail of state changes

### 3. Clarity
- State flow is explicit and documented
- Easy to understand business process
- Self-documenting code

### 4. Testability
- Each state can be tested independently
- Easy to verify state transitions
- Clear success/failure paths

### 5. Maintainability
- Adding new states is straightforward
- Refactoring is safe with type checking
- Changes are localized

---

## Integration with Existing Code

### Replace Current Cart States
```csharp
// Before
IStareCos state = new ValidatedCos(true);

// After
ShoppingCartEntity.IShoppingCart state = new ShoppingCartEntity.ValidatedShoppingCart(...);
```

### Use in Aggregates
```csharp
public class ShoppingCartAggregate
{
    private ShoppingCartEntity.IShoppingCart _currentState;
    
    public void Validate() 
    {
        if (_currentState is ShoppingCartEntity.UnvalidatedShoppingCart unvalidated)
        {
            // Transform to validated
            _currentState = new ShoppingCartEntity.ValidatedShoppingCart(...);
        }
    }
}
```

---

## Summary

? **3 Entity State Machines Created:**
1. **ShoppingCart** - 5 states (Unvalidated ? Validated ? Payed ? Completed / Invalid)
2. **Order** - 7 states (Unvalidated ? Validated ? Placed ? Shipped ? Delivered / Cancelled / Invalid)
3. **Customer** - 6 states (Unvalidated ? Validated ? Active ? Premium / Inactive / Invalid)

? **All Following DDD Patterns:**
- Immutable records
- Internal constructors
- IReadOnlyCollection for lists
- Interface for pattern matching
- Invalid state with Reasons
- Value objects integration

? **Production Ready:**
- Type-safe
- Immutable
- Well-documented
- Testable
- Maintainable

**The entity states are ready to be integrated into your domain operations and workflows!** ??

---

*Documentation Date: January 2024*  
*Pattern Source: `copilot_instructions_full.md`*  
*Framework: .NET 9, C# 13*  
*Architecture: Domain-Driven Design*
