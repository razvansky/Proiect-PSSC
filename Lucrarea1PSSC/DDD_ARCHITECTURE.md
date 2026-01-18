# Domain-Driven Design Architecture Documentation

## Overview
This document describes the complete DDD architecture for the E-commerce Shopping System, including Commands, Aggregates, Events, Validation Rules, and Invariants.

---

## 1. Shopping Cart Bounded Context ??

### Aggregate Root: `CosDeCumparaturi`

#### Commands

| Command | Description | Validation Rules |
|---------|-------------|------------------|
| `CreateCartCommand` | Creates a new shopping cart | - Customer name cannot be empty<br>- Customer must exist |
| `AddProductToCartCommand` | Adds product to cart | - Product name cannot be empty<br>- Cart cannot be PayedCos<br>- Cart cannot be UnvalidatedCos<br>- Product must exist in catalog<br>- Product stock must be > 0 |
| `RemoveProductFromCartCommand` | Removes product from cart | - Product name cannot be empty<br>- Cart cannot be PayedCos<br>- Cart cannot be UnvalidatedCos<br>- Product must exist in cart |
| `EmptyCartCommand` | Empties entire cart | - Cart cannot be PayedCos<br>- Cart cannot be UnvalidatedCos<br>- Cart cannot be EmptyCos |
| `PayCartCommand` | Pays for cart | - Cart cannot be already paid<br>- Cart cannot be UnvalidatedCos<br>- Cart cannot be EmptyCos |

#### Domain Events

```csharp
- CosCreatEvent                    // Cart created
- ProdusAdaugatInCosEvent         // Product added (PUBLISHED to Inventory)
- ProdusStergeDinCosEvent         // Product removed (PUBLISHED to Inventory)
- CosGolitEvent                   // Cart emptied (PUBLISHED to Inventory)
- StareCosSchimbataEvent          // Cart state changed
- CosValidatEvent                 // Cart validated
- CosPlatitEvent                  // Cart paid (PUBLISHED to Order Management)
```

#### Invariants

1. ? **Paid Cart Immutability**: A paid cart cannot be modified
2. ? **State Consistency**: Cart state must reflect content (Empty/Validated/Payed)
3. ? **Atomic Stock Operations**: Stock changes are atomic with cart operations
4. ? **Non-empty Payment**: An empty cart cannot be paid
5. ? **Valid Cart Operations**: Invalid carts cannot be used

#### Usage Example

```csharp
// Create command with validation
var (success, command, error) = CreateCartCommand.TryCreate("John Doe");
if (!success)
{
    Console.WriteLine($"Error: {error}");
    return;
}

// Execute command
var cart = new CosDeCumparaturi();
// ... cart operations
```

---

## 2. Inventory Management Bounded Context ??

### Aggregate Root: `ProdusAggregate`

#### Commands

| Command | Description | Validation Rules |
|---------|-------------|------------------|
| `DecreaseStockCommand` | Decreases product stock | - Product code must be positive<br>- Quantity must be positive<br>- Sufficient stock must be available |
| `IncreaseStockCommand` | Increases product stock | - Product code must be positive<br>- Quantity must be positive |

#### Domain Events

```csharp
- StocProdusScazutEvent          // Stock decreased
- StocProdusMaritEvent           // Stock increased
- ProdusEpuizatEvent            // Product out of stock (PUBLISHED)
- ProdusDisponibilEvent         // Product available again
```

#### Invariants

1. ? **Non-negative Stock**: Stock cannot be negative
2. ? **Positive Price**: Price must be positive
3. ? **Immutable Product Code**: Product code is unique and cannot change
4. ? **Atomic Operations**: Stock operations are atomic

#### Usage Example

```csharp
// Create product aggregate
var product = ProdusAggregate.Create(
    new CodProdus(1),
    "Laptop",
    new UnitQuantity(10),
    new KilogramQuantity(2.5),
    new Price(2500.00)
);

// Decrease stock (returns event)
var decreaseEvent = product.DecreaseStock(1);
EventBus.Publish(decreaseEvent);
```

---

## 3. Order Management Bounded Context ??

### Aggregate Root: `ComandaAggregate`

#### Commands

| Command | Description | Validation Rules |
|---------|-------------|------------------|
| `PlaceOrderCommand` | Places order from paid cart | - Person cannot be null<br>- Cart cannot be null<br>- Cart must be in PayedCos state<br>- Delivery address must be valid (min 5 chars)<br>- Cart must contain products |

#### Domain Events

```csharp
- ComandaPlasataSuccessEvent     // Order placed successfully
- ComandaPlasataFailedEvent      // Order placement failed
- AdresaValidataEvent            // Address validated
- AdresaInvalidaEvent            // Address invalid
- ComandaPregatitaPentruPlasareEvent  // Order ready for placement
```

#### Order States

```csharp
public enum StaraComanda
{
    Plasata,        // Order placed
    InPregatire,    // Order being prepared
    Expediata,      // Order shipped
    Livrata,        // Order delivered
    Anulata         // Order cancelled
}
```

#### Invariants

1. ? **Paid Cart Requirement**: Order can only be created from paid carts
2. ? **Valid Delivery Address**: Address must be validated before order placement
3. ? **Immutable Total**: Order total is calculated at creation and cannot change
4. ? **Immutable Products**: Products in order cannot be modified after placement
5. ? **State Transition Rules**: Order state follows valid transitions

#### Usage Example

```csharp
// Create order from paid cart
var (success, order, error) = ComandaAggregate.CreateFromPaidCart(persoana, cos);

if (success)
{
    var successEvent = order.ToSuccessEvent();
    EventBus.Publish(successEvent);
}
else
{
    var failedEvent = new ComandaEvent.ComandaPlasataFailedEvent(error);
    // Handle error
}
```

---

## 4. Customer Management Bounded Context ??

### Aggregate Root: `Persoana`

#### Commands

| Command | Description | Validation Rules |
|---------|-------------|------------------|
| `AssociateCartToCustomerCommand` | Associates cart with customer | - Customer name cannot be empty<br>- Cart cannot be null<br>- Customer must exist |

#### Domain Events

```csharp
- CosAsociatClientuluiEvent      // Cart associated with customer
- IstoricCosActualizatEvent      // Cart history updated
```

#### Invariants

1. ? **Valid Customer Name**: Customer must have a valid name
2. ? **Valid Email**: Email address must be valid
3. ? **Current Cart**: Current cart is always the most recent
4. ? **Immutable History**: Old carts in history cannot be modified

---

## Event Flow Diagram

```
CreateCartCommand ? CosCreatEvent
                 ?
AddProductCommand ? ProdusAdaugatInCosEvent ? StocProdusScazutEvent
                 ?
                CosValidatEvent
                 ?
PayCartCommand ? CosPlatitEvent
                 ?
PlaceOrderCommand ? ComandaPlasataSuccessEvent
```

---

## Cross-Context Communication

### Event Bus Pattern

Events are published and consumed across contexts using the EventBus:

```csharp
// Shopping Cart publishes
EventBus.Publish(new CartEvents.ProdusAdaugatInCosEvent(...));

// Inventory subscribes
EventBus.Subscribe<CartEvents.ProdusAdaugatInCosEvent>(evt => 
{
    // Decrease stock
    product.DecreaseStock(evt.Cantitate);
});
```

### Published Events (Cross-Context)

| Source Context | Event | Target Context | Action |
|----------------|-------|----------------|--------|
| Shopping Cart | `ProdusAdaugatInCosEvent` | Inventory | Decrease stock |
| Shopping Cart | `ProdusStergeDinCosEvent` | Inventory | Increase stock |
| Shopping Cart | `CosGolitEvent` | Inventory | Restore all stock |
| Shopping Cart | `CosPlatitEvent` | Order Management | Start order workflow |
| Inventory | `ProdusEpuizatEvent` | Admin/UI | Show warning |
| Order Management | `ComandaPlasataSuccessEvent` | Customer | Update order history |

---

## Validation Rules Summary

### Cart Operations
- ? Cannot modify paid carts
- ? Cannot use invalid carts
- ? Product names required
- ? Product must exist in catalog
- ? Sufficient stock required

### Inventory Operations
- ? Stock cannot go negative
- ? Product codes must be valid
- ? Quantities must be positive

### Order Operations
- ? Only paid carts can create orders
- ? Valid delivery address required
- ? Order must contain products
- ? State transitions must follow rules

### Customer Operations
- ? Customer name required
- ? Valid email required
- ? Cart must be valid

---

## Implementation Checklist

- ? Commands created with validation
- ? Aggregates with invariants
- ? Domain events defined
- ? Event bus for cross-context communication
- ? Validation rules documented
- ? State transitions defined
- ? Factory methods for safe creation
- ? Read-only properties for encapsulation

---

## Next Steps

1. **Integrate Commands** into existing Program.cs
2. **Implement EventBus** subscriptions
3. **Add Event Sourcing** for audit trail
4. **Create Unit Tests** for aggregates
5. **Add Domain Exceptions** for better error handling
6. **Implement Repositories** for persistence
7. **Add Sagas** for complex workflows
8. **Create Read Models** for queries

---

## References

- Domain-Driven Design by Eric Evans
- Implementing Domain-Driven Design by Vaughn Vernon
- Event Sourcing Pattern
- CQRS Pattern
- Aggregate Pattern
