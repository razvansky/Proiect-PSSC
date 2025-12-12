# Integration Guide: Using Commands and Aggregates in Program.cs

This guide shows how to refactor existing code to use the new DDD architecture.

---

## Setup EventBus Subscriptions (Add to Main startup)

```csharp
private static void SetupEventBusSubscriptions()
{
    // Inventory subscribes to cart events
    EventBus.Subscribe<CartEvents.ProdusAdaugatInCosEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] Product added to cart: {evt.NumeProdus}, Stock decreased");
    });

    EventBus.Subscribe<CartEvents.ProdusStergeDinCosEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] Product removed from cart: {evt.NumeProdus}, Stock increased");
    });

    EventBus.Subscribe<CartEvents.CosPlatitEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] Cart paid by {evt.NumePersoana}, Total: {evt.TotalPlatit} lei");
    });

    // Order management subscribes to cart payment
    EventBus.Subscribe<CartEvents.CosPlatitEvent>(evt =>
    {
        Console.WriteLine($"[EVENT] Order ready for placement for {evt.NumePersoana}");
    });

    // Inventory subscribes to stock events
    EventBus.Subscribe<InventoryEvents.ProdusEpuizatEvent>(evt =>
    {
        Console.WriteLine($"[WARNING] Product {evt.NumeProdus} is OUT OF STOCK!");
    });

    EventBus.Subscribe<InventoryEvents.ProdusDisponibilEvent>(evt =>
    {
        Console.WriteLine($"[INFO] Product {evt.NumeProdus} is now AVAILABLE (Stock: {evt.StocDisponibil})");
    });
}
```

---

## Refactored Case Statements

### Case 1: Create Cart (WITH COMMANDS)

**Before:**
```csharp
case 1:
    try {
        Console.WriteLine("Introduceti numele persoanei:");
        string numePersoana = Console.ReadLine();
        if(string.IsNullOrWhiteSpace(numePersoana))
        {
            throw new ArgumentException("Numele persoanei nu poate fi gol");
        }
        var persoana = persoane.FirstOrDefault(p => p.Nume.Name == numePersoana);
        if(persoana==null)
        {
            throw new InvalidOperationException("Persoana nu exista");
        }
       
        cos = new CosDeCumparaturi();
        Console.WriteLine("Cos creat cu succes");
        persoane[persoane.IndexOf(persoana)] = persoana.AdaugaCos(cos);
        Console.WriteLine($"Cos adaugat {numePersoana} cu succes");
    }
    catch(InvalidOperationException ex)
    {
        Console.WriteLine(ex.Message);
        break;
    }
    break;
```

**After (With Commands and Events):**
```csharp
case 1:
    Console.WriteLine("Introduceti numele persoanei:");
    string numePersoana = Console.ReadLine();
    
    // Use command with built-in validation
    var (success, command, error) = 
        Lucrarea1PSSC.clase.ClaseCos.Commands.CreateCartCommand.TryCreate(numePersoana);
    
    if (!success)
    {
        Console.WriteLine($"Eroare: {error}");
        break;
    }

    var persoana = persoane.FirstOrDefault(p => p.Nume.Name == command.NumePersoana);
    if (persoana == null)
    {
        Console.WriteLine("Persoana nu exista");
        break;
    }

    // Create cart
    cos = new CosDeCumparaturi();
    Console.WriteLine("Cos creat cu succes");

    // Update person
    var updatedPersoana = persoana.AdaugaCos(cos);
    persoane[persoane.IndexOf(persoana)] = updatedPersoana;

    // Publish domain event
    EventBus.Publish(new CartEvents.CosCreatEvent(
        command.NumePersoana, 
        DateTime.UtcNow
    ));

    Console.WriteLine($"Cos adaugat {command.NumePersoana} cu succes");
    break;
```

---

### Case 2: Add Product (WITH COMMANDS)

**Before:**
```csharp
case 2:
    numeProdus=Console.ReadLine();
    try
    {
        if(cos==null)
        {
            throw new NullReferenceException("Cosul nu a fost creat");
        }
    }
    catch(NullReferenceException ex)
    {
        Console.WriteLine(ex.Message);
        break;
    }
    cos.AdaugaProdus(numeProdus,produse);
    break;
```

**After (With Commands and Events):**
```csharp
case 2:
    Console.WriteLine("Introduceti numele produsului:");
    numeProdus = Console.ReadLine();

    if (cos == null)
    {
        Console.WriteLine("Cosul nu a fost creat");
        break;
    }

    // Use command with validation
    var (addSuccess, addCommand, addError) = 
        Lucrarea1PSSC.clase.ClaseCos.Commands.AddProductToCartCommand.TryCreate(numeProdus);

    if (!addSuccess)
    {
        Console.WriteLine($"Eroare: {addError}");
        break;
    }

    // Execute domain logic
    cos.AdaugaProdus(addCommand.NumeProdus, produse);

    // Event is published automatically by AdaugaProdus if you modify it
    // Or publish manually:
    var produs = produse.FirstOrDefault(p => p.Nume == addCommand.NumeProdus);
    if (produs != null)
    {
        EventBus.Publish(new CartEvents.ProdusAdaugatInCosEvent(
            produs.Nume,
            produs.CodProdus.Cod,
            1,
            produs.Pret.pret,
            DateTime.UtcNow
        ));
    }
    break;
```

---

### Case 9: Pay Cart (WITH COMMANDS)

**Before:**
```csharp
case 9:
    cos.platesteCos();
    break;
```

**After (With Commands and Events):**
```csharp
case 9:
    if (cos == null)
    {
        Console.WriteLine("Cosul nu a fost creat");
        break;
    }

    // Use command
    var payCommand = Lucrarea1PSSC.clase.ClaseCos.Commands.PayCartCommand.Create();

    // Execute payment
    bool paymentSuccess = cos.platesteCos();

    if (paymentSuccess)
    {
        // Find current person
        var currentPerson = persoane.FirstOrDefault(p => p.CosCurent == cos);
        if (currentPerson != null)
        {
            // Publish payment event
            EventBus.Publish(new CartEvents.CosPlatitEvent(
                currentPerson.Nume.Name,
                cos.TotalCos(),
                cos.GetProduseCos().Count,
                DateTime.UtcNow
            ));
        }
    }
    break;
```

---

### Case 10: Place Order (WITH AGGREGATE)

**Before:**
```csharp
case 10:
    Console.WriteLine("Introduceti numele persoanei:");
    string numePersoanaComanda = Console.ReadLine();
    var persoanaComanda = persoane.FirstOrDefault(p => p.Nume.Name == numePersoanaComanda);
    if(persoanaComanda==null)
    {
        Console.WriteLine("Persoana nu exista");
        break;
    }
    if (persoanaComanda.CosCurent == null)
    {
        Console.WriteLine("Persoana nu are cos curent");
        break;
    }
    var eventres = PlasareComandaWorkflow.PlaseazaComanda(persoanaComanda, persoanaComanda.CosCurent);
    
    switch (eventres)
    {
        case ComandaEvent.ComandaPlasataSuccessEvent success:
            Console.WriteLine(success.Message);
            Console.WriteLine($"Detalii: Total {success.TotalComanda} lei, {success.NumarProduse} produse");
            break;
        case ComandaEvent.ComandaPlasataFailedEvent failed:
            Console.WriteLine(failed.Message);
            break;
    }
    break;
```

**After (With Aggregate and Commands):**
```csharp
case 10:
    Console.WriteLine("Introduceti numele persoanei:");
    string numePersoanaComanda = Console.ReadLine();
    
    var persoanaComanda = persoane.FirstOrDefault(p => p.Nume.Name == numePersoanaComanda);
    if (persoanaComanda == null)
    {
        Console.WriteLine("Persoana nu exista");
        break;
    }

    if (persoanaComanda.CosCurent == null)
    {
        Console.WriteLine("Persoana nu are cos curent");
        break;
    }

    // Use PlaceOrderCommand for validation
    var (cmdSuccess, placeOrderCmd, cmdError) = 
        Lucrarea1PSSC.clase.Workflow.Commands.PlaceOrderCommand.TryCreate(
            persoanaComanda, 
            persoanaComanda.CosCurent
        );

    if (!cmdSuccess)
    {
        Console.WriteLine($"Eroare validare: {cmdError}");
        break;
    }

    // Use ComandaAggregate to create order with invariants
    var (orderSuccess, order, orderError) = 
        Lucrarea1PSSC.clase.Workflow.ComandaAggregate.CreateFromPaidCart(
            persoanaComanda, 
            persoanaComanda.CosCurent
        );

    if (orderSuccess)
    {
        // Order created successfully
        var successEvent = order.ToSuccessEvent();
        
        Console.WriteLine(successEvent.Message);
        Console.WriteLine($"Order ID: {order.ComandaId}");
        Console.WriteLine($"Total: {order.Total} lei");
        Console.WriteLine($"Produse: {order.NumarProduse}");
        Console.WriteLine($"Stare: {order.Stare}");
        Console.WriteLine($"Data plasare: {order.DataPlasare:dd/MM/yyyy HH:mm:ss}");

        // Publish success event
        EventBus.Publish(successEvent);
    }
    else
    {
        // Order creation failed
        var failedEvent = new ComandaEvent.ComandaPlasataFailedEvent(orderError);
        Console.WriteLine(failedEvent.Message);
        
        // Publish failure event
        EventBus.Publish(failedEvent);
    }
    break;
```

---

## Complete Main Method with EventBus

```csharp
private static void Main(string[] args)
{
    // Setup event bus subscriptions
    SetupEventBusSubscriptions();

    List<Produs> produse = CitireFisiere.CitireProduseDinJson(@"..\..\..\resources\produsemagazin.json");
    List<Persoana> persoane = CitireFisiere.CitirePersoaneDinJson(@"..\..\..\resources\persoane.json");
    CosDeCumparaturi cos = new CosDeCumparaturi(1);
    
    Console.WriteLine("=== E-Commerce Shopping System ===");
    Console.WriteLine("Event-Driven Architecture with DDD\n");

    // ... rest of the code with refactored cases
}

private static void SetupEventBusSubscriptions()
{
    // ... as shown above
}
```

---

## Using Product Aggregate (Optional Enhancement)

If you want to use the Product Aggregate for better stock management:

```csharp
// Convert existing products to aggregates
var productAggregates = produse.Select(p => 
    ProdusAggregate.Create(
        p.CodProdus,
        p.Nume,
        p.Quantity,
        p.Kilogram,
        p.Pret
    )
).ToList();

// When adding to cart:
var product = productAggregates.FirstOrDefault(p => p.Nume == numeProdus);
if (product != null && product.IsAvailable())
{
    var decreaseEvent = product.DecreaseStock(1);
    EventBus.Publish(decreaseEvent);
    
    if (product.IsOutOfStock())
    {
        var outOfStockEvent = new InventoryEvents.ProdusEpuizatEvent(
            product.CodProdus.Cod,
            product.Nume,
            DateTime.UtcNow
        );
        EventBus.Publish(outOfStockEvent);
    }
    
    cos.AdaugaProdus(numeProdus, produse);
}
else
{
    Console.WriteLine("Produsul nu este disponibil sau nu exista");
}
```

---

## Benefits of This Approach

### 1. **Validation is Centralized**
- Commands have built-in validation
- No duplicate validation logic in UI

### 2. **Clear Separation of Concerns**
- Commands = User Intent
- Aggregates = Business Logic
- Events = What Happened

### 3. **Audit Trail**
- All events can be logged
- Complete history of system actions

### 4. **Testability**
- Commands can be unit tested independently
- Aggregates enforce invariants
- Events provide clear contracts

### 5. **Extensibility**
- Easy to add new event subscribers
- New features can listen to existing events
- No modification of existing code

---

## Event Logging Example

Add this to enhance observability:

```csharp
private static void SetupEventBusSubscriptions()
{
    // Log all cart events
    EventBus.Subscribe<CartEvents.ICartEvent>(evt =>
    {
        Console.WriteLine($"[CART EVENT] {evt.GetType().Name} at {evt.Timestamp:HH:mm:ss}");
    });

    // Log all inventory events
    EventBus.Subscribe<InventoryEvents.IInventoryEvent>(evt =>
    {
        Console.WriteLine($"[INVENTORY EVENT] {evt.GetType().Name} at {evt.Timestamp:HH:mm:ss}");
    });

    // Log all order events
    EventBus.Subscribe<ComandaEvent.IComandaEvent>(evt =>
    {
        Console.WriteLine($"[ORDER EVENT] {evt.GetType().Name}");
    });

    // Specific business logic handlers
    EventBus.Subscribe<CartEvents.ProdusAdaugatInCosEvent>(evt =>
    {
        Console.WriteLine($"  ? Product '{evt.NumeProdus}' added, Stock affected");
    });

    EventBus.Subscribe<InventoryEvents.ProdusEpuizatEvent>(evt =>
    {
        Console.WriteLine($"  ?? ALERT: Product '{evt.NumeProdus}' is OUT OF STOCK!");
    });
}
```

---

## Summary

? **Commands** provide validation and encapsulate user intent  
? **Aggregates** enforce business rules and invariants  
? **Events** communicate what happened across contexts  
? **EventBus** decouples components  
? **Factory Methods** ensure safe object creation  

This architecture makes your code:
- More maintainable
- Easier to test
- Better documented through events
- Ready for future enhancements (Event Sourcing, CQRS, Microservices)

?? **Your E-commerce system is now following Domain-Driven Design best practices!**
