using Microsoft.AspNetCore.Mvc;
using Lucrarea1PSSC.api.DTOs;
using Lucrarea1PSSC.api.Services;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace Lucrarea1PSSC.api.Controllers
{
    /// <summary>
    /// Shopping Cart API Controller
    /// Provides endpoints for cart operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Manage shopping carts, add products, and process payments with real-time inventory tracking")]
    public class CartController : ControllerBase
    {
        private readonly CartApiService _cartService;

        public CartController(CartApiService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// View shopping cart for a customer
        /// </summary>
        /// <param name="customerName">Name of the customer</param>
        /// <returns>Cart details with all items</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/cart/view/Ion Popescu
        ///
        /// This endpoint returns the complete shopping cart including:
        /// - Cart status (Empty, Validated, Paid)
        /// - List of all items with quantities and prices
        /// - Total amount and item count
        /// 
        /// **Available Test Customers:**
        /// - Ion Popescu
        /// - Maria Ionescu
        /// - Andrei Stanciu
        /// - Elena Radu
        /// - Mihai Popa
        /// </remarks>
        /// <response code="200">Returns the shopping cart</response>
        /// <response code="400">If the customer name is invalid</response>
        /// <response code="404">If the customer is not found</response>
        [HttpGet("view/{customerName}")]
        [SwaggerOperation(
            Summary = "??? View Customer's Shopping Cart",
            Description = "Retrieves the complete shopping cart with all items, prices, and status",
            OperationId = "GetCart",
            Tags = new[] { "Cart" }
        )]
        [SwaggerResponse(200, "Cart retrieved successfully", typeof(ViewCartResponse))]
        [SwaggerResponse(400, "Invalid request", typeof(ErrorResponse))]
        [SwaggerResponse(404, "Customer not found", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(ViewCartResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> ViewCart([FromRoute] string customerName)
        {
            var (success, response, error) = await _cartService.ViewCartAsync(customerName);

            if (!success)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = error ?? "Failed to view cart",
                    Timestamp = System.DateTime.UtcNow
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Add a product to the shopping cart
        /// </summary>
        /// <param name="request">Product details to add</param>
        /// <returns>Updated cart information</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/cart/add-product
        ///     {
        ///        "customerName": "Ion Popescu",
        ///        "productName": "Laptop Dell XPS 15",
        ///        "quantity": 1
        ///     }
        ///
        /// This endpoint will:
        /// - Validate customer exists
        /// - Check product availability and stock
        /// - Add product to cart if stock is available
        /// - Update cart total
        /// - Publish domain event
        /// 
        /// **Available Products:**
        /// - Laptop Dell XPS 15 (5499.99 RON, Stock: 10)
        /// - Mouse Logitech MX Master (349.99 RON, Stock: 50)
        /// - Keyboard Mechanical RGB (599.99 RON, Stock: 30)
        /// - Monitor LG 27" 4K (1899.99 RON, Stock: 15)
        /// - And 6 more products...
        /// </remarks>
        /// <response code="200">Product added successfully</response>
        /// <response code="400">If the request is invalid or stock is insufficient</response>
        /// <response code="404">If customer or product not found</response>
        [HttpPost("add-product")]
        [SwaggerOperation(
            Summary = "? Add Product to Cart",
            Description = "Adds a product to the shopping cart with automatic stock validation",
            OperationId = "AddProduct",
            Tags = new[] { "Cart" }
        )]
        [SwaggerResponse(200, "Product added to cart", typeof(AddProductToCartResponse))]
        [SwaggerResponse(400, "Invalid request or insufficient stock", typeof(ErrorResponse))]
        [SwaggerResponse(404, "Customer or product not found", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(AddProductToCartResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> AddProductToCart([FromBody] AddProductToCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid request",
                    Details = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            var (success, response, error) = await _cartService.AddProductToCartAsync(request);

            if (!success)
            {
                // Determine if it's a not found or bad request
                if (error != null && (error.Contains("not found") || error.Contains("not exist")))
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = error,
                        Timestamp = System.DateTime.UtcNow
                    });
                }

                return BadRequest(new ErrorResponse
                {
                    Error = error ?? "Failed to add product to cart",
                    Timestamp = System.DateTime.UtcNow
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Mark shopping cart as paid
        /// </summary>
        /// <param name="request">Payment details</param>
        /// <returns>Payment confirmation</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/cart/mark-paid
        ///     {
        ///        "customerName": "Ion Popescu",
        ///        "paymentMethod": "Card",
        ///        "transactionId": "TXN-123456789"
        ///     }
        ///
        /// This endpoint will:
        /// - Validate cart state (must be Validated, not Empty or already Paid)
        /// - Process payment
        /// - Mark cart as paid
        /// - Publish payment event
        /// - Return transaction confirmation
        /// 
        /// **Payment Methods:**
        /// - Card (Credit/Debit)
        /// - Cash
        /// - Bank Transfer
        /// - Digital Wallet
        /// </remarks>
        /// <response code="200">Cart marked as paid successfully</response>
        /// <response code="400">If cart state is invalid or payment fails</response>
        /// <response code="404">If customer not found</response>
        [HttpPost("mark-paid")]
        [SwaggerOperation(
            Summary = "?? Mark Cart as Paid",
            Description = "Processes payment and marks the shopping cart as paid",
            OperationId = "PayCart",
            Tags = new[] { "Cart" }
        )]
        [SwaggerResponse(200, "Payment processed successfully", typeof(MarkCartAsPaidResponse))]
        [SwaggerResponse(400, "Invalid cart state or payment failed", typeof(ErrorResponse))]
        [SwaggerResponse(404, "Customer not found", typeof(ErrorResponse))]
        [ProducesResponseType(typeof(MarkCartAsPaidResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> MarkCartAsPaid([FromBody] MarkCartAsPaidRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Invalid request",
                    Details = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            var (success, response, error) = await _cartService.MarkCartAsPaidAsync(request);

            if (!success)
            {
                if (error != null && error.Contains("not found"))
                {
                    return NotFound(new ErrorResponse
                    {
                        Error = error,
                        Timestamp = System.DateTime.UtcNow
                    });
                }

                return BadRequest(new ErrorResponse
                {
                    Error = error ?? "Failed to mark cart as paid",
                    Timestamp = System.DateTime.UtcNow
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get status of all active carts (admin/debug endpoint)
        /// </summary>
        /// <returns>Dictionary of customer names and their cart statuses</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/cart/active-carts
        ///
        /// Returns a dictionary with customer names as keys and cart statuses as values.
        /// 
        /// **Cart Statuses:**
        /// - Empty: No items in cart
        /// - Validated: Has items, ready for checkout
        /// - Paid: Payment completed
        /// - Unvalidated: Cart in invalid state
        /// </remarks>
        /// <response code="200">Returns dictionary of active carts</response>
        [HttpGet("active-carts")]
        [SwaggerOperation(
            Summary = "?? View All Active Carts",
            Description = "Admin endpoint to view status of all carts in the system",
            OperationId = "GetActiveCarts",
            Tags = new[] { "Cart", "Admin" }
        )]
        [SwaggerResponse(200, "Active carts retrieved", typeof(Dictionary<string, string>))]
        [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
        public async Task<IActionResult> GetActiveCarts()
        {
            var carts = await _cartService.GetActiveCartsStatusAsync();
            return Ok(carts);
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>API health status</returns>
        /// <remarks>
        /// Simple health check to verify API is running.
        /// 
        /// Returns:
        /// - Status: "Healthy"
        /// - Service name
        /// - Current timestamp
        /// </remarks>
        /// <response code="200">API is healthy</response>
        [HttpGet("health")]
        [SwaggerOperation(
            Summary = "?? API Health Check",
            Description = "Verify the API is running and responsive",
            OperationId = "HealthCheck",
            Tags = new[] { "System" }
        )]
        [SwaggerResponse(200, "API is healthy")]
        [ProducesResponseType(200)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "Cart API",
                Timestamp = System.DateTime.UtcNow,
                Version = "1.0.0",
                Environment = "Development"
            });
        }
    }
}
