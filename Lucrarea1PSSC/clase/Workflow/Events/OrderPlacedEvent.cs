using System;
using Lucrarea1PSSC.clase.Infrastructure.Messaging;

namespace Lucrarea1PSSC.clase.Workflow.Events
{
    /// <summary>
    /// Event emitted when an order is placed and ready for processing
    /// This triggers both invoice generation and delivery initiation
    /// </summary>
    public class OrderPlacedEvent : IMessage
    {
        public Guid MessageId { get; }
        public DateTime Timestamp { get; }
        public string MessageType => "OrderPlaced";

        // Order details
        public Guid OrderNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public DateTime OrderDate { get; set; }

        // Cart details
        public List<OrderItemInfo> Items { get; set; } = new();

        public OrderPlacedEvent()
        {
            MessageId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }

        public OrderPlacedEvent(
            Guid orderNumber,
            string customerName,
            string customerEmail,
            string deliveryAddress,
            decimal totalAmount,
            int totalItems,
            List<OrderItemInfo> items)
        {
            MessageId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            
            OrderNumber = orderNumber;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            DeliveryAddress = deliveryAddress;
            TotalAmount = totalAmount;
            TotalItems = totalItems;
            OrderDate = DateTime.UtcNow;
            Items = items;
        }

        public override string ToString()
        {
            return $"Order {OrderNumber} placed by {CustomerName} - Total: {TotalAmount:C} RON ({TotalItems} items)";
        }
    }

    /// <summary>
    /// Order item information for events
    /// </summary>
    public class OrderItemInfo
    {
        public int ProductCode { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
