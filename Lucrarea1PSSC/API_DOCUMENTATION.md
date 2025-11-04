# Shopping Cart REST API Documentation

## Overview

This REST API provides endpoints for managing shopping carts in an e-commerce system, implementing **Domain-Driven Design (DDD)** and **Event-Driven Architecture** patterns.

## Base URL

```
Development: http://localhost:5000 or https://localhost:5001
```

## Swagger UI

Interactive API documentation available at the root URL:
```
http://localhost:5000
```

---

## Endpoints

### 1. View Shopping Cart

**GET** `/api/cart/view/{customerName}`

View the current shopping cart for a specific customer.

#### Parameters

| Name | Type | Location | Required | Description |
|------|------|----------|----------|-------------|
| customerName | string | path | Yes | Name of the customer |

#### Response 200 (Success)

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
      "kilogramQuantity": 1.0
    }
  ],
  "totalAmount": 5499.99,
  "totalItems": 1,
  "lastModified": "2024-01-15T10:30:00Z"
}
```

#### Response 400 (Bad Request)

```json
{
  "error": "Customer name is required",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Response 404 (Not Found)

```json
{
  "error": "Customer 'John Doe' not found",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Cart Status Values

- **Empty**: Cart has no items
- **Validated**: Cart has items and is ready for checkout
- **Paid**: Cart has been paid
- **Unvalidated**: Cart is in invalid state

#### Example Request (cURL)

```bash
curl -X GET "http://localhost:5000/api/cart/view/Ion%20Popescu" \
  -H "accept: application/json"
```

#### Example Request (PowerShell)

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/cart/view/Ion%20Popescu" `
  -Method Get `
  -ContentType "application/json"
```

---

### 2. Add Product to Cart

**POST** `/api/cart/add-product`

Add a product to the shopping cart. If the cart doesn't exist, it will be created automatically.

#### Request Body

```json
{
  "customerName": "Ion Popescu",
  "productName": "Laptop Dell XPS 15",
  "quantity": 1
}
```

#### Request Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| customerName | string | Yes | Name of the customer |
| productName | string | Yes | Name of the product to add |
| quantity | number | No | Quantity to add (default: 1) |

#### Response 200 (Success)

```json
{
  "success": true,
  "message": "Product 'Laptop Dell XPS 15' added to cart successfully",
  "addedItem": {
    "productCode": 1001,
    "productName": "Laptop Dell XPS 15",
    "quantity": 1,
    "unitPrice": 5499.99,
    "lineTotal": 5499.99,
    "quantityType": "Unit",
    "kilogramQuantity": 1.0
  },
  "newCartTotal": 5499.99,
  "totalItems": 1
}
```

#### Response 400 (Bad Request)

```json
{
  "error": "Insufficient stock for 'Laptop Dell XPS 15'. Available: 0",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Response 404 (Not Found)

```json
{
  "error": "Product 'iPhone 15' not found in database",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Example Request (cURL)

```bash
curl -X POST "http://localhost:5000/api/cart/add-product" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Ion Popescu",
    "productName": "Laptop Dell XPS 15",
    "quantity": 1
  }'
```

#### Example Request (PowerShell)

```powershell
$body = @{
    customerName = "Ion Popescu"
    productName = "Laptop Dell XPS 15"
    quantity = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/cart/add-product" `
  -Method Post `
  -Body $body `
  -ContentType "application/json"
```

#### Example Request (JavaScript/Fetch)

```javascript
fetch('http://localhost:5000/api/cart/add-product', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    customerName: 'Ion Popescu',
    productName: 'Laptop Dell XPS 15',
    quantity: 1
  })
})
.then(response => response.json())
.then(data => console.log(data));
```

---

### 3. Mark Cart as Paid

**POST** `/api/cart/mark-paid`

Mark the shopping cart as paid, completing the checkout process.

#### Request Body

```json
{
  "customerName": "Ion Popescu",
  "paymentMethod": "Card",
  "transactionId": "TXN-123456789"
}
```

#### Request Schema

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| customerName | string | Yes | Name of the customer |
| paymentMethod | string | No | Payment method (default: "Card") |
| transactionId | string | No | Transaction ID (auto-generated if not provided) |

#### Response 200 (Success)

```json
{
  "success": true,
  "message": "Cart paid successfully for Ion Popescu",
  "totalPaid": 5499.99,
  "itemsPaid": 1,
  "paymentDate": "2024-01-15T10:30:00Z",
  "transactionId": "TXN-123456789"
}
```

#### Response 400 (Bad Request - Cart Already Paid)

```json
{
  "error": "Cart is already paid",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Response 400 (Bad Request - Empty Cart)

```json
{
  "error": "Cart is empty. Add products before payment",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Response 404 (Not Found)

```json
{
  "error": "No active cart found for customer",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Example Request (cURL)

```bash
curl -X POST "http://localhost:5000/api/cart/mark-paid" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Ion Popescu",
    "paymentMethod": "Card",
    "transactionId": "TXN-123456789"
  }'
```

#### Example Request (PowerShell)

```powershell
$body = @{
    customerName = "Ion Popescu"
    paymentMethod = "Card"
    transactionId = "TXN-123456789"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/cart/mark-paid" `
  -Method Post `
  -Body $body `
  -ContentType "application/json"
```

---

### 4. Get Active Carts (Admin)

**GET** `/api/cart/active-carts`

Get status of all active carts in the system (for admin/debugging purposes).

#### Response 200 (Success)

```json
{
  "Ion Popescu": "Validated",
  "Maria Ionescu": "Paid",
  "Andrei Stanciu": "Empty"
}
```

#### Example Request (cURL)

```bash
curl -X GET "http://localhost:5000/api/cart/active-carts" \
  -H "accept: application/json"
```

---

### 5. Health Check

**GET** `/api/cart/health`

Check if the Cart API is running.

#### Response 200 (Success)

```json
{
  "status": "Healthy",
  "service": "Cart API",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

#### Example Request (cURL)

```bash
curl -X GET "http://localhost:5000/api/cart/health" \
  -H "accept: application/json"
```

---

## Complete Workflow Example

### Step 1: View Empty Cart

```bash
curl -X GET "http://localhost:5000/api/cart/view/Ion%20Popescu"
```

**Response:**
```json
{
  "customerName": "Ion Popescu",
  "cartStatus": "Empty",
  "items": [],
  "totalAmount": 0,
  "totalItems": 0
}
```

### Step 2: Add Product to Cart

```bash
curl -X POST "http://localhost:5000/api/cart/add-product" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Ion Popescu",
    "productName": "Laptop Dell XPS 15"
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Product 'Laptop Dell XPS 15' added to cart successfully",
  "addedItem": {
    "productCode": 1001,
    "productName": "Laptop Dell XPS 15",
    "quantity": 1,
    "unitPrice": 5499.99,
    "lineTotal": 5499.99
  },
  "newCartTotal": 5499.99,
  "totalItems": 1
}
```

### Step 3: Add Another Product

```bash
curl -X POST "http://localhost:5000/api/cart/add-product" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Ion Popescu",
    "productName": "Mouse Logitech MX Master"
  }'
```

### Step 4: View Updated Cart

```bash
curl -X GET "http://localhost:5000/api/cart/view/Ion%20Popescu"
```

**Response:**
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
      "lineTotal": 5499.99
    },
    {
      "productCode": 1002,
      "productName": "Mouse Logitech MX Master",
      "quantity": 1,
      "unitPrice": 349.99,
      "lineTotal": 349.99
    }
  ],
  "totalAmount": 5849.98,
  "totalItems": 2
}
```

### Step 5: Mark Cart as Paid

```bash
curl -X POST "http://localhost:5000/api/cart/mark-paid" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "Ion Popescu",
    "paymentMethod": "Card"
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Cart paid successfully for Ion Popescu",
  "totalPaid": 5849.98,
  "itemsPaid": 2,
  "paymentDate": "2024-01-15T10:30:00Z",
  "transactionId": "auto-generated-guid"
}
```

---

## Error Handling

All error responses follow this format:

```json
{
  "error": "Error message description",
  "details": "Optional additional details",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Common HTTP Status Codes

- **200 OK**: Request successful
- **400 Bad Request**: Invalid request data or business rule violation
- **404 Not Found**: Resource not found (customer, product, cart)
- **500 Internal Server Error**: Unexpected server error

---

## Event-Driven Architecture

The API publishes domain events for all cart operations:

- **CosCreatEvent**: When a new cart is created
- **ProdusAdaugatInCosEvent**: When a product is added to cart
- **CosPlatitEvent**: When a cart is marked as paid

These events are logged to the console and can be subscribed to by other services.

---

## Available Customers (Seeded Data)

The following customers are available in the system:

1. **Ion Popescu** - ion.popescu@example.com
2. **Maria Ionescu** - maria.ionescu@example.com
3. **Andrei Stanciu** - andrei.stanciu@example.com
4. **Elena Radu** - elena.radu@example.com
5. **Mihai Popa** - mihai.popa@example.com

## Available Products (Seeded Data)

The following products are available:

1. **Laptop Dell XPS 15** - 5499.99 RON (Stock: 10)
2. **Mouse Logitech MX Master** - 349.99 RON (Stock: 50)
3. **Keyboard Mechanical RGB** - 599.99 RON (Stock: 30)
4. **Monitor LG 27" 4K** - 1899.99 RON (Stock: 15)
5. **Laptop Lenovo ThinkPad** - 4299.99 RON (Stock: 8)
6. **Webcam Logitech C920** - 449.99 RON (Stock: 25)
7. **Headset HyperX Cloud II** - 499.99 RON (Stock: 40)
8. **SSD Samsung 1TB** - 699.99 RON (Stock: 60)
9. **RAM Corsair 16GB DDR4** - 399.99 RON (Stock: 45)
10. **GPU RTX 4070** - 3999.99 RON (Stock: 5)

---

## Testing with Postman

### Import Collection

Create a Postman collection with these requests:

1. **View Cart**: GET `{{baseUrl}}/api/cart/view/Ion Popescu`
2. **Add Product**: POST `{{baseUrl}}/api/cart/add-product`
3. **Mark Paid**: POST `{{baseUrl}}/api/cart/mark-paid`
4. **Active Carts**: GET `{{baseUrl}}/api/cart/active-carts`
5. **Health Check**: GET `{{baseUrl}}/api/cart/health`

Set environment variable:
- `baseUrl`: `http://localhost:5000`

---

## Technical Details

### Architecture Patterns

- **Domain-Driven Design (DDD)**: Aggregate roots, entities, value objects
- **Event-Driven Architecture**: Domain events published for all operations
- **CQRS**: Separate read and write operations
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management

### Technologies

- **.NET 9.0**
- **ASP.NET Core Web API**
- **Entity Framework Core 9.0**
- **SQL Server / LocalDB**
- **Swagger/OpenAPI**

### Database Integration

- Products and customers are loaded from the database
- Stock is verified before adding products to cart
- All cart operations maintain data consistency

---

## Running the API

### Start the Server

```bash
dotnet run --project Lucrarea1PSSC
```

### Access Swagger UI

```
http://localhost:5000
```

### Check Health

```bash
curl http://localhost:5000/api/cart/health
```

---

## Troubleshooting

### Database Connection Issues

If you see database errors:

1. Check that SQL Server / LocalDB is running
2. Verify connection string in `appsettings.json`
3. See `SQL_SERVER_SETUP_GUIDE.md` for detailed setup

### Customer Not Found

Ensure you're using exact customer names from the seeded data (case-sensitive).

### Product Not Found

Check product names match exactly with seeded data.

### Cart Already Paid

Once a cart is paid, you need to create a new cart for the customer.

---

## Support

For issues or questions, check the console logs for detailed event tracking and error messages.
