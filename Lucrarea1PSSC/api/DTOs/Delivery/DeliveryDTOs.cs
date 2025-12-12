using System;

namespace Lucrarea1PSSC.api.DTOs.Delivery
{
    /// <summary>
    /// Request to schedule a delivery
    /// </summary>
    public class DeliveryRequest
    {
        public Guid OrderNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Romania";
        public string Phone { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public DateTime OrderDate { get; set; }
        public string Priority { get; set; } = "Standard"; // Standard, Express, Next-Day
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from delivery service
    /// </summary>
    public class DeliveryResponse
    {
        public bool Success { get; set; }
        public string DeliveryId { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime EstimatedDeliveryDate { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Delivery status update
    /// </summary>
    public class DeliveryStatusUpdate
    {
        public string DeliveryId { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Pending, PickedUp, InTransit, OutForDelivery, Delivered
        public string Location { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
