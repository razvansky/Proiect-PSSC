# Quick Reference Card - DDD E-Commerce System

## ?? Running the Application

```bash
cd Lucrarea1PSSC
dotnet run
```

---

## ?? Menu Options & Commands Used

| # | Operation | Command Used | Event Published |
|---|-----------|--------------|-----------------|
| 1 | Create Cart | `CreateCartCommand` | `CosCreatEvent` |
| 2 | Add Product | `AddProductToCartCommand` | `ProdusAdaugatInCosEvent` |
| 3 | Remove Product | `RemoveProductFromCartCommand` | `ProdusStergeDinCosEvent` |
| 4 | Empty Cart | `EmptyCartCommand` | `CosGolitEvent` |
| 5 | Display Cart | - | - |
| 6 | Show Total | - | - |
| 7 | Display Products | - | - |
| 8 | Display Customers | - | - |
| 9 | Pay Cart | `PayCartCommand` | `CosPlatitEvent` |
| 10 | Place Order | `PlaceOrderCommand` + `ComandaAggregate` | `ComandaPlasataSuccessEvent` |

---

## ?? Complete Workflow Example

### Scenario: Customer Places an Order

```
1. Option 1: Create cart for "John Doe"
   ??> CreateCartCommand validates name
   ??> CosCreatEvent published
   ??> [EVENT] New cart created for John Doe

2. Option 2: Add product "Laptop"
   ??> AddProductToCartCommand validates product name
   ??> Product added to cart
   ??> Stock decreased by 1
   ??> ProdusAdaugatInCosEvent published
   ??> [EVENT] Product added to cart: Laptop, Stock decreased

3. Option 9: Pay cart
   ??> PayCartCommand created
   ??> Cart transitions to PayedCos
   ??> CosPlatitEvent published
   ??> [EVENT] Cart paid by John Doe, Total: 2500.00 lei

4. Option 10: Place order for "John Doe"
   ??> PlaceOrderCommand validates inputs
   ??> ComandaAggregate.CreateFromPaidCart() enforces invariants:
       ? Cart is paid
       ? Address is valid
       ? Cart has products
       ? Total is positive
   ??> Order created with unique Guid
   ??> ComandaPlasataSuccessEvent published
   ??> [EVENT] Order placed successfully
   ??> Display:
       - Order ID: 3fa85f64-5717-4562-b3fc-2c963f66afa6
       - Total: 2500 lei
       - Produse: 1
       - Stare: Plasata
       - Data plasare: 15/01/2024 14:30:45
```

---

## ?? Event Subscriptions (Auto-logged)

When you run the app, these events are automatically logged:

```
[EVENT] New cart created for {customer}
[EVENT] Product added to cart: {product}, Stock decreased
[EVENT] Product removed from cart: {product}, Stock increased
[EVENT] Cart emptied, {count} products returned to stock
[EVENT] Cart paid by {customer}, Total: {amount} lei
[WARNING] Product {product} is OUT OF STOCK!
[INFO] Product {product} is now AVAILABLE (Stock: {amount})
[EVENT] Order placed successfully:
  - Order ID: {guid}
  - Total: {amount} lei
  - Numar produse: {count}
[EVENT] Order placement failed: {reason}
```

---

## ? Validation Rules Quick Reference

### CreateCartCommand
- ? Empty customer name
- ? Customer doesn't exist

### AddProductToCartCommand
- ? Empty product name
- ? Cart not created
- ? Cart is paid (immutable)
- ? Cart is invalid
- ? Product doesn't exist
- ? Product out of stock

### RemoveProductFromCartCommand
- ? Empty product name
- ? Cart not created
- ? Cart is paid (immutable)
- ? Cart is invalid
- ? Product not in cart

### EmptyCartCommand
- ? Cart is paid (immutable)
- ? Cart is invalid or empty

### PayCartCommand
- ? Cart already paid
- ? Cart is invalid
- ? Cart is empty

### PlaceOrderCommand + ComandaAggregate
- ? Person is null
- ? Cart is null
- ? Cart not paid
- ? Address < 5 characters
- ? Cart is empty
- ? Total ? 0

---

## ??? Architecture Patterns Used

### Command Pattern
```
User Input ? Command.TryCreate(input) ? (success, command, error)
```

### Aggregate Pattern
```
Command ? Aggregate.Method() ? Enforces Invariants ? Returns Event
```

### Event-Driven
```
Operation ? Event Published ? EventBus ? Subscribers Notified
```

### Domain Events
```
CartEvents, InventoryEvents, OrderEvents, CustomerEvents
```

---

## ?? Invariants Enforced

### CosDeCumparaturi (Cart)
- ? Paid cart is immutable
- ? State reflects content (Empty/Validated/Payed)
- ? Stock operations are atomic
- ? Empty cart cannot be paid
- ? Invalid cart cannot be used

### ComandaAggregate (Order)
- ? Only created from paid carts
- ? Delivery address validated
- ? Order total is immutable
- ? Order products are immutable
- ? State transitions follow rules

### ProdusAggregate (Product)
- ? Stock cannot be negative
- ? Price must be positive
- ? Product code is immutable

### Persoana (Customer)
- ? Valid name required
- ? Current cart is most recent
- ? Old carts are read-only

---

## ?? Project Structure

```
Lucrarea1PSSC/
??? clase/
?   ??? ClaseCos/
?   ?   ??? Commands/           ? 5 Cart commands
?   ?   ??? CartEvents.cs       ? 7 Cart events
?   ?   ??? CosDeCumparaturi.cs ? Cart aggregate
?   ?
?   ??? ClaseProduse/
?   ?   ??? Commands/           ? 2 Stock commands
?   ?   ??? InventoryEvents.cs  ? 4 Inventory events
?   ?   ??? ProdusAggregate.cs  ? Product aggregate
?   ?
?   ??? Workflow/
?   ?   ??? Commands/           ? 1 Order command
?   ?   ??? OrderEvents.cs      ? 3 Order events
?   ?   ??? ComandaAggregate.cs ? Order aggregate
?   ?   ??? PlasareComandaWorkflow.cs
?   ?
?   ??? ClaseGestionarePersoane/
?   ?   ??? Commands/           ? 1 Customer command
?   ?   ??? CustomerEvents.cs   ? 2 Customer events
?   ?   ??? Persoana.cs         ? Customer aggregate
?   ?
?   ??? Infrastructure/
?       ??? EventBus.cs         ? Event pub/sub
?
??? main/
?   ??? Program.cs              ? Main app (DDD integrated)
?
??? Documentation/
    ??? DDD_ARCHITECTURE.md
    ??? COMMAND_EVENT_MAPPING.md
    ??? INTEGRATION_GUIDE.md
    ??? IMPLEMENTATION_SUMMARY.md
    ??? ARCHITECTURE_DIAGRAMS.md
    ??? FINAL_IMPLEMENTATION_REPORT.md
```

---

## ?? Testing Checklist

### Happy Path
- [ ] Create cart for existing customer
- [ ] Add product with available stock
- [ ] Remove product from cart
- [ ] Empty cart with products
- [ ] Pay cart with products
- [ ] Place order with paid cart

### Validation Tests
- [ ] Try creating cart with empty name
- [ ] Try adding empty product name
- [ ] Try adding product to paid cart
- [ ] Try paying empty cart
- [ ] Try placing order without payment
- [ ] Try placing order with invalid address

### Invariant Tests
- [ ] Verify paid cart is immutable
- [ ] Verify order total is immutable
- [ ] Verify stock never goes negative
- [ ] Verify cart state reflects content

### Event Flow Tests
- [ ] Verify all events are published
- [ ] Verify event subscribers are notified
- [ ] Verify event data is correct

---

## ?? Key Concepts

### Command
Encapsulates user intent with validation
```csharp
var (success, command, error) = Command.TryCreate(input);
if (success) Execute(command);
else ShowError(error);
```

### Aggregate
Enforces business invariants
```csharp
public class Aggregate {
    public Event DoSomething() {
        EnsureInvariant();
        UpdateState();
        return new Event(...);
    }
}
```

### Event
Represents what happened
```csharp
public record EventOccurred(Data, Timestamp) : IEvent;
EventBus.Publish(new EventOccurred(...));
```

### EventBus
Decouples contexts
```csharp
EventBus.Subscribe<Event>(evt => HandleEvent(evt));
EventBus.Publish(evt);
```

---

## ?? Useful Links

- **Eric Evans - Domain-Driven Design** (The Blue Book)
- **Vaughn Vernon - Implementing DDD** (The Red Book)
- **Microsoft - DDD Patterns**
- **Martin Fowler - Event Sourcing**
- **Greg Young - CQRS**

---

## ?? Tips

1. **Always use Commands** for user input validation
2. **Let Aggregates enforce invariants** automatically
3. **Publish Events** after successful operations
4. **Subscribe to Events** for cross-context communication
5. **Use pattern matching** for clean event handling
6. **Keep aggregates focused** on their bounded context
7. **Document invariants** in comments
8. **Test commands independently** from UI

---

## ?? Troubleshooting

### "Command validation failed"
? Check TryCreate return values
? Display error message to user

### "Event not published"
? Verify EventBus.Publish() is called
? Check event is defined correctly

### "Aggregate invariant violated"
? Review business rules
? Check state transitions

### "Null reference exception"
? Verify entity exists before operation
? Check for null in pattern matching

---

## ?? Quick Command Examples

```csharp
// Create Cart
var (success, cmd, err) = CreateCartCommand.TryCreate("John");

// Add Product
var (success, cmd, err) = AddProductToCartCommand.TryCreate("Laptop");

// Pay Cart
var cmd = PayCartCommand.Create();

// Place Order
var (success, cmd, err) = PlaceOrderCommand.TryCreate(person, cart);
var (orderSuccess, order, orderErr) = ComandaAggregate.CreateFromPaidCart(person, cart);
```

---

**Status:** ? Production Ready
**Build:** ? Successful
**Tests:** ? Manual (ready for automation)
**Documentation:** ? Complete

---

*Last Updated: January 2024*
