# ?? Complete Testing Guide - Task 3: Message Queues & Event-Driven Architecture

## ?? Prerequisites

Before testing, ensure:
- ? .NET 9 SDK installed
- ? SQL Server running (optional - works without DB)
- ? Project builds successfully
- ? Port 5000 is available

---

## ?? Quick Test (5 Minutes)

### Step 1: Start the Application

```bash
cd Lucrarea1PSSC
dotnet run
```

### Step 2: Watch Console Output

You should immediately see **demonstrations** running automatically:

```
????????????????????????????????????????????????????????????????
  DEMONSTRATING MESSAGE QUEUE PATTERNS
????????????????????????????????????????????????????????????????

?????????????????????????????????????????????????????????????????
?    DEMONSTRATION: 1-to-1 QUEUE COMMUNICATION                  ?
?????????????????????????????????????????????????????????????????

[MESSAGE QUEUE] Created queue 'demo-queue' with capacity 1000
[PRODUCER] Sending messages to queue...
[MESSAGE QUEUE 'demo-queue'] Published: OrderPlaced (ID: abc-123)
[MESSAGE QUEUE 'demo-queue'] Published: OrderPlaced (ID: def-456)
[MESSAGE QUEUE 'demo-queue'] Published: OrderPlaced (ID: ghi-789)

[QUEUE STATUS] Messages in queue: 3

[CONSUMER] Consuming messages from queue...
[MESSAGE QUEUE 'demo-queue'] Consumed: OrderPlaced (ID: abc-123)
[CONSUMER] Processing: Order abc-123 placed by Customer 1 - Total: 100 RON (1 items)
...

?????????????????????????????????????????????????????????????????
?    DEMONSTRATION: 1-to-MANY TOPIC COMMUNICATION               ?
?????????????????????????????????????????????????????????????????

[MESSAGE TOPIC] Created topic 'demo-topic'
[MESSAGE TOPIC 'demo-topic'] New subscriber added. Total subscribers: 1
[MESSAGE TOPIC 'demo-topic'] New subscriber added. Total subscribers: 2
[MESSAGE TOPIC 'demo-topic'] New subscriber added. Total subscribers: 3

[TOPIC STATUS] Total subscribers: 3

[MESSAGE TOPIC 'demo-topic'] Publishing OrderPlaced to 3 subscribers
[SUBSCRIBER 1 - INVOICE] Processing order...
[SUBSCRIBER 2 - DELIVERY] Processing order...
[SUBSCRIBER 3 - EMAIL] Processing order...
[SUBSCRIBER 1 - INVOICE] ? Invoice generated
[SUBSCRIBER 2 - DELIVERY] ? Delivery scheduled
[SUBSCRIBER 3 - EMAIL] ? Confirmation email sent
```

**? If you see this output, the message queue infrastructure is working!**

### Step 3: Open Browser

Navigate to: **http://localhost:5000**

You should see the Swagger UI with beautiful custom styling.

---

## ?? Full End-to-End Test

### Test Scenario: Complete Order Flow

This tests **all workflows together**:
1. Add product to cart
2. Pay cart
3. **Triggers OrderPlacedEvent**
4. **Invoice generated automatically**
5. **Delivery scheduled automatically**

---

### Option A: Using Swagger UI (Recommended)

#### Step 1: View Available Customers

1. Open **http://localhost:5000**
2. Find `GET /api/cart/active-carts`
3. Click **"Try it out"** ? **"Execute"**

**Expected Response:**
```json
{
  "Ion Popescu": "Empty",
  "Maria Ionescu": "Empty",
  "Andrei Stanciu": "Empty"
}
```

#### Step 2: Add Product to Cart

1. Find `POST /api/cart/add-product`
2. Click **"Try it out"**
3. Use this request body:

```json
{
  "customerName": "Ion Popescu",
  "productName": "Laptop Dell XPS 15",
  "quantity": 1
}
```

4. Click **"Execute"**

**Expected Response:**
```json
{
  "success": true,
  "message": "Product 'Laptop Dell XPS 15' added successfully",
  "addedItem": {
    "productCode": 1001,
    "productName": "Laptop Dell XPS 15",
    "quantity": 1,
    "unitPrice": 5499.99,
    "lineTotal": 5499.99,
    "quantityType": "Unit",
    "kilogramQuantity": 1
  },
  "newCartTotal": 5499.99,
  "totalItems": 1
}
```

**Console Output:**
```
[CartApiService] Loading products and customers...
[CartApiService] Created 5 products, 3 customers
```

#### Step 3: View Cart

1. Find `GET /api/cart/view/{customerName}`
2. Click **"Try it out"**
3. Enter: `Ion Popescu`
4. Click **"Execute"**

**Expected Response:**
```json
{
  "customerName": "Ion Popescu",
  "cartStatus": "Validated",
  "items": [
    {
      "productCode": 1001,
      "productName": "Laptop Dell XPS 15",
      "quantity": 1,
      "unitPrice": 5499.99,
      "lineTotal": 5499.99,
      "quantityType": "Unit",
      "kilogramQuantity": 1
    }
  ],
  "totalAmount": 5499.99,
  "totalItems": 1,
  "lastModified": "2025-01-17T..."
}
```

#### Step 4: Pay Cart (THE MAIN EVENT!)

1. Find `POST /api/cart/mark-paid`
2. Click **"Try it out"**
3. Use this request body:

```json
{
  "customerName": "Ion Popescu",
  "transactionId": "TEST-TXN-12345"
}
```

4. Click **"Execute"**

**Expected Response:**
```json
{
  "success": true,
  "message": "Cart paid successfully for Ion Popescu. Invoice and delivery initiated.",
  "totalPaid": 5499.99,
  "itemsPaid": 1,
  "paymentDate": "2025-01-17T...",
  "transactionId": "TEST-TXN-12345"
}
```

---

### ?? What Happens in the Console

This is where the **magic** happens! Watch the console carefully:

```
?????????????????????????????????????????????????????????????????
?         ORDER PROCESSING STARTED                              ?
?????????????????????????????????????????????????????????????????
Order:    123e4567-e89b-12d3-a456-426614174000
Customer: Ion Popescu
Total:    5,499.99 RON
Items:    1
???????????????????????????????????????????????????????????????

[WORKFLOW] Emitting OrderPlacedEvent...
[MESSAGE TOPIC 'order-placed'] Publishing OrderPlaced to 2 subscribers

??????????????????????????????????????????????????????????????
?  PARALLEL PROCESSING - Both workflows run simultaneously   ?
??????????????????????????????????????????????????????????????

[ORCHESTRATOR] Processing OrderPlacedEvent for order 123e4567...
[ORCHESTRATOR] Triggering Invoice Generation workflow...
[INVOICE WORKFLOW] Starting invoice generation for order 123e4567...
[INVOICE WORKFLOW] ? Invoice generated: INV-20250117-001001
[INVOICE WORKFLOW] Subtotal: 5,499.99 RON
[INVOICE WORKFLOW] VAT (19%): 1,044.99 RON
[INVOICE WORKFLOW] Total: 6,544.98 RON

?????????????????????????????????????????????????????????
?               E-COMMERCE INVOICE                      ?
?????????????????????????????????????????????????????????

Invoice Number: INV-20250117-001001
Order Number:   123e4567-e89b-12d3-a456-426614174000
Invoice Date:   2025-01-17 14:30:45
Due Date:       2025-02-16

BILL TO:
  Ion Popescu
  Str. Mihai Eminescu 15
  Email: ion.popescu@example.com

ITEMS:
?????????????????????????????????????????????????????????
Description                    Qty      Price      Total
?????????????????????????????????????????????????????????
Laptop Dell XPS 15               1    5499.99    5499.99
?????????????????????????????????????????????????????????
Subtotal:                                        5499.99 RON
VAT (19%):                                       1044.99 RON
?????????????????????????????????????????????????????????
TOTAL:                                           6544.98 RON
?????????????????????????????????????????????????????????

Thank you for your business!

[MESSAGE TOPIC 'invoice-generated'] Publishing InvoiceGenerated...

---

[ORCHESTRATOR] Processing OrderPlacedEvent for order 123e4567...
[ORCHESTRATOR] Triggering Delivery Initiation workflow...
[DELIVERY WORKFLOW] Starting delivery initiation for order 123e4567...
[DELIVERY WORKFLOW] Using simulated delivery scheduling...
[DELIVERY WORKFLOW] ? Delivery scheduled successfully
[DELIVERY WORKFLOW] Tracking Number: TRK-20250117-123456
[DELIVERY WORKFLOW] Carrier: FAN Courier Express
[DELIVERY WORKFLOW] Estimated Delivery: 2025-01-18

?????????????????????????????????????????????????????????
?              DELIVERY SHIPPING LABEL                  ?
?????????????????????????????????????????????????????????

Tracking Number: TRK-20250117-123456
Carrier:         FAN Courier Express
Order Number:    123e4567-e89b-12d3-a456-426614174000

SHIP TO:
  Ion Popescu
  Str. Mihai Eminescu 15

Package Details:
  Items:     1
  Value:     5499.99 RON
  Priority:  EXPRESS

Estimated Delivery: 2025-01-18
?????????????????????????????????????????????????????????

[MESSAGE TOPIC 'delivery-initiated'] Publishing DeliveryInitiated...

---

[ORCHESTRATOR] Invoice INV-20250117-001001 generated
[ORCHESTRATOR] Could send email to ion.popescu@example.com...

[ORCHESTRATOR] Delivery TRK-20250117-123456 initiated for Ion Popescu
[ORCHESTRATOR] Could send tracking SMS/email...

[ORCHESTRATOR] ? Order processing complete - all workflows triggered
???????????????????????????????????????????????????????????????
```

---

### Option B: Using cURL

```bash
# Add product
curl -X POST "http://localhost:5000/api/cart/add-product" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Ion Popescu",
    "productName": "Laptop Dell XPS 15",
    "quantity": 1
  }'

# Pay cart (triggers workflows)
curl -X POST "http://localhost:5000/api/cart/mark-paid" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Ion Popescu",
    "transactionId": "TEST-001"
  }'
```

---

### Option C: Using PowerShell

```powershell
# Add product
$body = @{
    customerName = "Ion Popescu"
    productName = "Laptop Dell XPS 15"
    quantity = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/cart/add-product" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

# Pay cart
$payBody = @{
    customerName = "Ion Popescu"
    transactionId = "TEST-001"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/cart/mark-paid" `
    -Method Post `
    -Body $payBody `
    -ContentType "application/json"
```

---

## ? Verification Checklist

After running the test, verify you see:

### Console Output
- [ ] Queue demonstration completed
- [ ] Topic demonstration completed
- [ ] Orchestrator initialized with 2 subscribers
- [ ] OrderPlacedEvent published to topic
- [ ] Invoice workflow triggered
- [ ] Full invoice printed with VAT calculation
- [ ] Delivery workflow triggered
- [ ] Shipping label printed
- [ ] "Order processing complete" message

### Invoice Output
- [ ] Invoice number format: INV-YYYYMMDD-NNNNNN
- [ ] Customer name and address present
- [ ] Product line items listed
- [ ] Subtotal calculated correctly
- [ ] VAT (19%) calculated correctly
- [ ] Total = Subtotal + VAT

### Delivery Output
- [ ] Tracking number format: TRK-YYYYMMDD-NNNNNN
- [ ] Carrier determined (Express if > 1000 RON)
- [ ] Customer name and address present
- [ ] Estimated delivery date shown
- [ ] Priority level correct

---

## ?? Troubleshooting

### Issue: No console output after payment

**Cause:** Orchestrator not registered or no subscribers

**Solution:**
1. Check `Program.cs` for:
   ```csharp
   builder.Services.AddSingleton<OrderProcessingOrchestrator>(...);
   ```
2. Restart the application

### Issue: "Object reference not set to an instance of an object"

**Cause:** Orchestrator is null in CartApiService

**Solution:**
1. Check dependency injection in `Program.cs`:
   ```csharp
   builder.Services.AddScoped<CartApiService>(serviceProvider =>
   {
       var dbService = serviceProvider.GetService<OrderWorkflowDatabaseService>();
       var deliveryClient = serviceProvider.GetService<DeliveryApiClient>();
       var orchestrator = serviceProvider.GetService<OrderProcessingOrchestrator>();
       return new CartApiService(dbService, deliveryClient, orchestrator);
   });
   ```

### Issue: Demonstrations don't run on startup

**Cause:** Missing await in Program.cs

**Solution:**
```csharp
await SimpleQueueExample.DemonstrateQueue();
await TopicExample.DemonstrateTopic();
```

### Issue: Polly retry not working

**Cause:** Delivery controller not returning 500 errors

**Check:** The mock delivery controller should simulate 20% failure rate:
```csharp
if (_random.Next(100) < 20)
{
    return StatusCode(500, new { error = "Temporary service unavailable" });
}
```

---

## ?? Test Scenarios

### Scenario 1: Low-Value Order (Standard Delivery)

**Product:** Mouse Logitech MX Master (349.99 RON)
**Expected Carrier:** Romanian Post
**Expected Priority:** STANDARD

```json
{
  "customerName": "Maria Ionescu",
  "productName": "Mouse Logitech MX Master",
  "quantity": 1
}
```

### Scenario 2: High-Value Order (Express Delivery)

**Product:** Laptop Dell XPS 15 (5499.99 RON)
**Expected Carrier:** FAN Courier Express
**Expected Priority:** EXPRESS

```json
{
  "customerName": "Ion Popescu",
  "productName": "Laptop Dell XPS 15",
  "quantity": 1
}
```

### Scenario 3: Multiple Products

Add multiple products, then pay:

```json
// Add product 1
{
  "customerName": "Andrei Stanciu",
  "productName": "Laptop Dell XPS 15",
  "quantity": 1
}

// Add product 2
{
  "customerName": "Andrei Stanciu",
  "productName": "Mouse Logitech MX Master",
  "quantity": 1
}

// Pay
{
  "customerName": "Andrei Stanciu"
}
```

**Expected:** Invoice shows 2 line items, total includes both products

---

## ?? Understanding the Flow

### Message Flow Diagram

```
API Request: POST /api/cart/mark-paid
        ?
CartApiService.MarkCartAsPaidAsync()
        ?
PlasareComandaWorkflow.PlaseazaComandaAsync()
        ?
Save Order to Database (optional)
        ?
EMIT: OrderPlacedEvent
        ?
OrderProcessingOrchestrator.ProcessOrderAsync()
        ?
Publish to Topic: "order-placed"
        ?
    ?????????????????????????????????
    ?               ?               ?
Subscriber 1    Subscriber 2    Subscriber 3
Invoice         Delivery        Email
Workflow        Workflow        Service
    ?               ?               ?
Generate        Schedule        Send
Invoice         Delivery        Notification
    ?               ?               ?
EMIT:           EMIT:           (Log)
InvoiceGen      DeliveryInit
Event           Event
    ?               ?
Email           SMS
Handler         Handler
```

### Event Types

| Event | Emitted By | Subscribers |
|-------|-----------|-------------|
| `OrderPlacedEvent` | PlasareComandaWorkflow | Invoice Workflow, Delivery Workflow |
| `InvoiceGeneratedEvent` | Invoice Workflow | Email Service |
| `DeliveryInitiatedEvent` | Delivery Workflow | SMS Service |

---

## ?? Performance Testing

### Load Test (Optional)

Test multiple orders in quick succession:

```bash
# Linux/Mac
for i in {1..10}; do
  curl -X POST "http://localhost:5000/api/cart/mark-paid" \
    -H "Content-Type: application/json" \
    -d "{\"customerName\": \"Ion Popescu\"}"
done
```

```powershell
# Windows PowerShell
1..10 | ForEach-Object {
    Invoke-RestMethod -Uri "http://localhost:5000/api/cart/mark-paid" `
        -Method Post `
        -Body '{"customerName":"Ion Popescu"}' `
        -ContentType "application/json"
}
```

**Expected:** All orders processed, all invoices and deliveries generated

---

## ?? Success Criteria

You have successfully tested Task 3 if:

- ? Queue demonstration runs on startup
- ? Topic demonstration runs on startup
- ? Orchestrator shows 2 subscribers
- ? Payment triggers OrderPlacedEvent
- ? Invoice is generated with correct VAT
- ? Delivery is scheduled with tracking number
- ? Both workflows run in parallel
- ? All console output is clear and formatted
- ? No errors or exceptions occur

---

## ?? Additional Resources

- **Full Documentation:** `TASK3_MESSAGE_QUEUES_IMPLEMENTATION.md`
- **Quick Reference:** `TASK3_QUICK_START.md`
- **Delivery API Guide:** `DELIVERY_API_IMPLEMENTATION.md`
- **Architecture:** See console output on startup

---

## ?? Pro Tips

1. **Watch the console carefully** - all the action happens there!
2. **Test with different product values** - see Express vs Standard delivery
3. **Try multiple orders** - verify parallel processing works
4. **Check timestamps** - workflows should run simultaneously
5. **Verify VAT calculation** - should always be 19%

---

## ?? Need Help?

If tests fail:
1. Check build succeeded: `dotnet build`
2. Verify all files exist (see file list in documentation)
3. Restart application: `Ctrl+C` then `dotnet run`
4. Check port 5000 is free: `netstat -ano | findstr :5000`
5. Review console for error messages

---

**?? Happy Testing! The event-driven architecture should work beautifully!** ??
