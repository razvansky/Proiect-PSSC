using Microsoft.AspNetCore.Mvc;
using Lucrarea1PSSC.api.DTOs.Delivery;
using System;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace Lucrarea1PSSC.api.Controllers
{
    /// <summary>
    /// Mock Delivery API Controller
    /// Simulates an external delivery service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("External delivery service endpoints (Mock implementation for testing)")]
    public class DeliveryController : ControllerBase
    {
        private static int _deliveryCounter = 1;
        private static Random _random = new Random();

        /// <summary>
        /// Schedule a new delivery
        /// </summary>
        [HttpPost("schedule")]
        [SwaggerOperation(
            Summary = "?? Schedule Delivery",
            Description = "Creates a new delivery request and returns tracking information",
            OperationId = "ScheduleDelivery",
            Tags = new[] { "Delivery" }
        )]
        [SwaggerResponse(200, "Delivery scheduled successfully", typeof(DeliveryResponse))]
        [SwaggerResponse(400, "Invalid request", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Internal server error (simulated for testing retries)")]
        public async Task<IActionResult> ScheduleDelivery([FromBody] DeliveryRequest request)
        {
            // Simulate random failures for testing retry policy (20% failure rate)
            if (_random.Next(100) < 20)
            {
                Console.WriteLine("[DELIVERY SERVICE] ⚠ Simulating transient error (will retry)...");
                return StatusCode(500, new { error = "Temporary service unavailable" });
            }

            await Task.Delay(200); // Simulate processing time

            var trackingNumber = $"TRK-{DateTime.Now:yyyyMMdd}-{_deliveryCounter++:D6}";
            var estimatedDelivery = DateTime.Now.AddDays(request.Priority switch
            {
                "Express" => 1,
                "Next-Day" => 1,
                _ => 3
            });

            var response = new DeliveryResponse
            {
                Success = true,
                DeliveryId = Guid.NewGuid().ToString(),
                TrackingNumber = trackingNumber,
                EstimatedDeliveryDate = estimatedDelivery,
                Carrier = DetermineCarrier(request.Priority),
                Status = "Pending",
                Message = $"Delivery scheduled successfully for {request.CustomerName}"
            };

            Console.WriteLine($"[DELIVERY SERVICE] ? Delivery scheduled: {trackingNumber}");
            Console.WriteLine($"[DELIVERY SERVICE] Estimated delivery: {estimatedDelivery:yyyy-MM-dd}");

            return Ok(response);
        }

        /// <summary>
        /// Get delivery status by tracking number
        /// </summary>
        [HttpGet("status/{trackingNumber}")]
        [SwaggerOperation(
            Summary = "?? Get Delivery Status",
            Description = "Retrieves current status of a delivery by tracking number",
            OperationId = "GetDeliveryStatus",
            Tags = new[] { "Delivery" }
        )]
        [SwaggerResponse(200, "Status retrieved successfully", typeof(DeliveryStatusUpdate))]
        [SwaggerResponse(404, "Tracking number not found")]
        public async Task<IActionResult> GetDeliveryStatus(string trackingNumber)
        {
            await Task.Delay(100); // Simulate processing

            // Simulate random status
            var statuses = new[] { "Pending", "PickedUp", "InTransit", "OutForDelivery", "Delivered" };
            var randomStatus = statuses[_random.Next(statuses.Length)];

            var statusUpdate = new DeliveryStatusUpdate
            {
                TrackingNumber = trackingNumber,
                Status = randomStatus,
                Location = "Distribution Center - Cluj-Napoca",
                UpdatedAt = DateTime.Now,
                Notes = $"Package is {randomStatus.ToLower()}"
            };

            Console.WriteLine($"[DELIVERY SERVICE] Status for {trackingNumber}: {randomStatus}");

            return Ok(statusUpdate);
        }

        /// <summary>
        /// Cancel a scheduled delivery
        /// </summary>
        [HttpPost("cancel")]
        [SwaggerOperation(
            Summary = "? Cancel Delivery",
            Description = "Cancels a scheduled delivery",
            OperationId = "CancelDelivery",
            Tags = new[] { "Delivery" }
        )]
        [SwaggerResponse(200, "Delivery cancelled successfully")]
        [SwaggerResponse(400, "Invalid request")]
        public async Task<IActionResult> CancelDelivery([FromBody] CancelDeliveryRequest request)
        {
            await Task.Delay(100);

            Console.WriteLine($"[DELIVERY SERVICE] Delivery {request.DeliveryId} cancelled. Reason: {request.Reason}");

            return Ok(new { success = true, message = "Delivery cancelled successfully" });
        }

        private string DetermineCarrier(string priority)
        {
            return priority switch
            {
                "Express" => "FAN Courier Express",
                "Next-Day" => "DHL Express",
                _ => "Romanian Post"
            };
        }
    }

    public class CancelDeliveryRequest
    {
        public string DeliveryId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
    }
}
