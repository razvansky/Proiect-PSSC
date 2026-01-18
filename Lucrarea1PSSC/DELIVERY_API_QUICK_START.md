# ?? Quick Start - Testing Delivery API with Polly Retry

## ? Run It Now

```bash
cd Lucrarea1PSSC
dotnet run
```

Open browser: **http://localhost:5000**

---

## ?? Test Scenario: Complete Order with Delivery

### Step 1: View Cart
```http
GET /api/cart/view/Ion Popescu
```

### Step 2: Add Product
```http
POST /api/cart/add-product
Content-Type: application/json

{
  "customerName": "Ion Popescu",
  "productName": "Laptop Dell XPS 15",
  "quantity": 1
}
```

### Step 3: Mark as Paid (Triggers Delivery with Retry)
```http
POST /api/cart/mark-paid
Content-Type: application/json

{
  "customerName": "Ion Popescu",
  "transactionId": "TEST-TXN-001"
}
```

**Response** (Look for `trackingNumber`!):
```json
{
  "success": true,
  "message": "Cart paid successfully for Ion Popescu. Tracking: TRK-20250117-000001",
  "totalPaid": 5499.99,
  "itemsPaid": 1,
  "paymentDate": "2025-01-17T08:30:00Z",
  "transactionId": "TEST-TXN-001",
  "trackingNumber": "TRK-20250117-000001"  ? DELIVERY SCHEDULED!
}
```

### Step 4: Check Delivery Status
```http
GET /api/delivery/status/TRK-20250117-000001
```

---

## ?? Console Output - What to Expect

### ? Scenario 1: Success (80% probability)

```
[WORKFLOW] Saving order to database...
[WORKFLOW] ? Order saved successfully
[WORKFLOW] Scheduling delivery...
[DELIVERY API] Scheduling delivery for order...
[DELIVERY SERVICE] ? Delivery scheduled: TRK-20250117-000001
[DELIVERY API] ? Delivery scheduled successfully
[CartApiService] ? Order placed with tracking: TRK-20250117-000001
```

### ?? Scenario 2: Retry Once (15% probability)

```
[WORKFLOW] Saving order to database...
[WORKFLOW] ? Order saved successfully
[WORKFLOW] Scheduling delivery...
[DELIVERY API] Scheduling delivery for order...
[DELIVERY SERVICE] ?? Simulating transient error (will retry)...
[POLLY RETRY] Attempt 1 after 2s delay  ? RETRY!
[DELIVERY API] Scheduling delivery for order...
[DELIVERY SERVICE] ? Delivery scheduled: TRK-20250117-000002
[DELIVERY API] ? Delivery scheduled successfully
[CartApiService] ? Order placed with tracking: TRK-20250117-000002
```

### ???? Scenario 3: Multiple Retries (4% probability)

```
[DELIVERY SERVICE] ?? Simulating transient error (will retry)...
[POLLY RETRY] Attempt 1 after 2s delay
[DELIVERY SERVICE] ?? Simulating transient error (will retry)...
[POLLY RETRY] Attempt 2 after 4s delay  ? 2nd RETRY!
[DELIVERY SERVICE] ? Delivery scheduled: TRK-20250117-000003
[CartApiService] ? Order placed with tracking: TRK-20250117-000003
```

---

## ?? Key Features Demonstrated

### Polly Retry Policy

- **3 retries** with exponential backoff
- **Delays**: 2s, 4s, 8s
- **Total max wait**: 14 seconds
- **Handles**: HTTP 5xx, 408, network failures

### Mock Delivery API

- **20% failure rate** to test retries
- **Automatic tracking number** generation
- **Status endpoints** for verification

### Integration Points

1. **Order Workflow** ? Calls delivery after order saved
2. **Polly** ? Handles retries automatically
3. **Tracking** ? Returned in payment response
4. **Circuit Breaker** ? Opens after 5 failures

---

## ?? Verify Polly is Working

### Watch for These Log Messages:

? **Retry happening**:
```
[POLLY RETRY] Attempt 1 after 2s delay
```

? **Exponential backoff**:
```
[POLLY RETRY] Attempt 1 after 2s delay
[POLLY RETRY] Attempt 2 after 4s delay
[POLLY RETRY] Attempt 3 after 8s delay
```

? **Circuit breaker**:
```
[POLLY CIRCUIT BREAKER] Circuit opened for 30s
```

---

## ?? Checklist

- [x] Polly package installed
- [x] Typed HttpClient configured
- [x] Retry policy: 3 attempts, exponential backoff
- [x] Circuit breaker: 5 failures, 30s duration
- [x] Delivery API integrated in workflow
- [x] Tracking number in payment response
- [x] Console logging for observability
- [x] Mock API with 20% failure rate
- [x] All endpoints tested and working

---

## ?? Success Indicators

When you test, you should see:

1. ? Payment succeeds
2. ? Order saved to database
3. ? Delivery API called
4. ? Tracking number in response
5. ? Retry logs if failure occurs
6. ? Final success even after retries

---

## ?? Support

- **Documentation**: See `DELIVERY_API_IMPLEMENTATION.md`
- **Swagger UI**: http://localhost:5000
- **Console**: Watch for `[POLLY RETRY]` messages

---

**?? Ready to test! Run `dotnet run` and try it!**
