using System;
using System.Collections.Generic;
using Lucrarea1PSSC.clase.ClaseProduse.ValueObjects;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane.ValueObjects;
using Lucrarea1PSSC.clase.Workflow.ValueObjects;

namespace Lucrarea1PSSC.clase.Workflow.Entities
{
    // Create entity states for Order following the pattern from copilot-instructions.md
    // 
    // State flow:
    // UnvalidatedOrder ? ValidatedOrder ? PlacedOrder ? ShippedOrder ? DeliveredOrder
    //                 ? InvalidOrder                  ? CancelledOrder
    //
    // States needed:
    // 1. UnvalidatedOrder: Raw input with string properties: customerName, deliveryAddress, products[]
    // 2. ValidatedOrder: After validation with value objects: Customer, Address, Products[], Total
    // 3. PlacedOrder: After placement with OrderNumber, PlacementDate
    // 4. ShippedOrder: After shipping with TrackingNumber, ShipmentDate
    // 5. DeliveredOrder: Final state with DeliveryDate, Signature
    // 6. CancelledOrder: Cancelled state with CancellationReason, CancellationDate
    // 7. InvalidOrder: When validation/processing fails, contains Reasons
    //
    // Each state implements IOrder interface
    // Use internal constructors and IReadOnlyCollection for lists
    
    /// <summary>
    /// Order entity with complete lifecycle states
    /// Represents the full order flow from creation to delivery
    /// </summary>
    public static class OrderEntity
    {
        /// <summary>
        /// Base interface for all order states
        /// </summary>
        public interface IOrder { }
        
        /// <summary>
        /// Initial unvalidated state - raw input data
        /// Contains string-based properties before validation
        /// </summary>
        public record UnvalidatedOrder : IOrder
        {
            internal UnvalidatedOrder(
                string customerName,
                string deliveryAddress,
                string email,
                IEnumerable<UnvalidatedOrderLine> orderLines)
            {
                CustomerName = customerName;
                DeliveryAddress = deliveryAddress;
                Email = email;
                OrderLines = orderLines;
            }
            
            public string CustomerName { get; }
            public string DeliveryAddress { get; }
            public string Email { get; }
            public IEnumerable<UnvalidatedOrderLine> OrderLines { get; }
        }
        
        /// <summary>
        /// Validated state - all data validated with value objects
        /// Order is ready to be placed
        /// </summary>
        public record ValidatedOrder : IOrder
        {
            internal ValidatedOrder(
                string customerId,
                EmailAddress customerEmail,
                DeliveryAddress deliveryAddress,
                IReadOnlyCollection<ValidatedOrderLine> orderLines,
                Money subtotal,
                Money shippingCost,
                Money tax,
                Money total,
                DateTime createdAt)
            {
                CustomerId = customerId;
                CustomerEmail = customerEmail;
                DeliveryAddress = deliveryAddress;
                OrderLines = orderLines;
                Subtotal = subtotal;
                ShippingCost = shippingCost;
                Tax = tax;
                Total = total;
                CreatedAt = createdAt;
            }
            
            public string CustomerId { get; }
            public EmailAddress CustomerEmail { get; }
            public DeliveryAddress DeliveryAddress { get; }
            public IReadOnlyCollection<ValidatedOrderLine> OrderLines { get; }
            public Money Subtotal { get; }
            public Money ShippingCost { get; }
            public Money Tax { get; }
            public Money Total { get; }
            public DateTime CreatedAt { get; }
        }
        
        /// <summary>
        /// Placed state - order has been confirmed and placed
        /// Contains order number and placement information
        /// </summary>
        public record PlacedOrder : IOrder
        {
            internal PlacedOrder(
                OrderNumber orderNumber,
                string customerId,
                EmailAddress customerEmail,
                DeliveryAddress deliveryAddress,
                IReadOnlyCollection<ValidatedOrderLine> orderLines,
                Money subtotal,
                Money shippingCost,
                Money tax,
                Money total,
                DateTime createdAt,
                DateTime placedAt,
                DateTime estimatedDeliveryDate)
            {
                OrderNumber = orderNumber;
                CustomerId = customerId;
                CustomerEmail = customerEmail;
                DeliveryAddress = deliveryAddress;
                OrderLines = orderLines;
                Subtotal = subtotal;
                ShippingCost = shippingCost;
                Tax = tax;
                Total = total;
                CreatedAt = createdAt;
                PlacedAt = placedAt;
                EstimatedDeliveryDate = estimatedDeliveryDate;
            }
            
            public OrderNumber OrderNumber { get; }
            public string CustomerId { get; }
            public EmailAddress CustomerEmail { get; }
            public DeliveryAddress DeliveryAddress { get; }
            public IReadOnlyCollection<ValidatedOrderLine> OrderLines { get; }
            public Money Subtotal { get; }
            public Money ShippingCost { get; }
            public Money Tax { get; }
            public Money Total { get; }
            public DateTime CreatedAt { get; }
            public DateTime PlacedAt { get; }
            public DateTime EstimatedDeliveryDate { get; }
        }
        
        /// <summary>
        /// Shipped state - order has been dispatched
        /// Contains shipping information and tracking number
        /// </summary>
        public record ShippedOrder : IOrder
        {
            internal ShippedOrder(
                OrderNumber orderNumber,
                string customerId,
                EmailAddress customerEmail,
                DeliveryAddress deliveryAddress,
                IReadOnlyCollection<ValidatedOrderLine> orderLines,
                Money total,
                DateTime createdAt,
                DateTime placedAt,
                DateTime shippedAt,
                string trackingNumber,
                string carrier,
                DateTime estimatedDeliveryDate)
            {
                OrderNumber = orderNumber;
                CustomerId = customerId;
                CustomerEmail = customerEmail;
                DeliveryAddress = deliveryAddress;
                OrderLines = orderLines;
                Total = total;
                CreatedAt = createdAt;
                PlacedAt = placedAt;
                ShippedAt = shippedAt;
                TrackingNumber = trackingNumber;
                Carrier = carrier;
                EstimatedDeliveryDate = estimatedDeliveryDate;
            }
            
            public OrderNumber OrderNumber { get; }
            public string CustomerId { get; }
            public EmailAddress CustomerEmail { get; }
            public DeliveryAddress DeliveryAddress { get; }
            public IReadOnlyCollection<ValidatedOrderLine> OrderLines { get; }
            public Money Total { get; }
            public DateTime CreatedAt { get; }
            public DateTime PlacedAt { get; }
            public DateTime ShippedAt { get; }
            public string TrackingNumber { get; }
            public string Carrier { get; }
            public DateTime EstimatedDeliveryDate { get; }
        }
        
        /// <summary>
        /// Delivered state - final successful state
        /// Order has been delivered to customer
        /// </summary>
        public record DeliveredOrder : IOrder
        {
            internal DeliveredOrder(
                OrderNumber orderNumber,
                string customerId,
                EmailAddress customerEmail,
                DeliveryAddress deliveryAddress,
                IReadOnlyCollection<ValidatedOrderLine> orderLines,
                Money total,
                DateTime createdAt,
                DateTime placedAt,
                DateTime shippedAt,
                DateTime deliveredAt,
                string trackingNumber,
                string carrier,
                string? recipientName,
                string? signature)
            {
                OrderNumber = orderNumber;
                CustomerId = customerId;
                CustomerEmail = customerEmail;
                DeliveryAddress = deliveryAddress;
                OrderLines = orderLines;
                Total = total;
                CreatedAt = createdAt;
                PlacedAt = placedAt;
                ShippedAt = shippedAt;
                DeliveredAt = deliveredAt;
                TrackingNumber = trackingNumber;
                Carrier = carrier;
                RecipientName = recipientName;
                Signature = signature;
            }
            
            public OrderNumber OrderNumber { get; }
            public string CustomerId { get; }
            public EmailAddress CustomerEmail { get; }
            public DeliveryAddress DeliveryAddress { get; }
            public IReadOnlyCollection<ValidatedOrderLine> OrderLines { get; }
            public Money Total { get; }
            public DateTime CreatedAt { get; }
            public DateTime PlacedAt { get; }
            public DateTime ShippedAt { get; }
            public DateTime DeliveredAt { get; }
            public string TrackingNumber { get; }
            public string Carrier { get; }
            public string? RecipientName { get; }
            public string? Signature { get; }
        }
        
        /// <summary>
        /// Cancelled state - order was cancelled
        /// Contains cancellation information
        /// </summary>
        public record CancelledOrder : IOrder
        {
            internal CancelledOrder(
                OrderNumber orderNumber,
                string customerId,
                EmailAddress customerEmail,
                IReadOnlyCollection<ValidatedOrderLine> orderLines,
                Money total,
                DateTime createdAt,
                DateTime placedAt,
                DateTime cancelledAt,
                string cancellationReason,
                string? cancelledBy,
                bool refundIssued)
            {
                OrderNumber = orderNumber;
                CustomerId = customerId;
                CustomerEmail = customerEmail;
                OrderLines = orderLines;
                Total = total;
                CreatedAt = createdAt;
                PlacedAt = placedAt;
                CancelledAt = cancelledAt;
                CancellationReason = cancellationReason;
                CancelledBy = cancelledBy;
                RefundIssued = refundIssued;
            }
            
            public OrderNumber OrderNumber { get; }
            public string CustomerId { get; }
            public EmailAddress CustomerEmail { get; }
            public IReadOnlyCollection<ValidatedOrderLine> OrderLines { get; }
            public Money Total { get; }
            public DateTime CreatedAt { get; }
            public DateTime PlacedAt { get; }
            public DateTime CancelledAt { get; }
            public string CancellationReason { get; }
            public string? CancelledBy { get; }
            public bool RefundIssued { get; }
        }
        
        /// <summary>
        /// Invalid state - validation or processing failed
        /// Contains reasons for failure
        /// </summary>
        public record InvalidOrder : IOrder
        {
            internal InvalidOrder(IEnumerable<string> reasons)
            {
                Reasons = reasons;
            }
            
            public IEnumerable<string> Reasons { get; }
        }
    }
    
    /// <summary>
    /// Unvalidated order line with raw string data
    /// </summary>
    public record UnvalidatedOrderLine
    {
        internal UnvalidatedOrderLine(
            string productName,
            int quantity,
            double unitPrice)
        {
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
        
        public string ProductName { get; }
        public int Quantity { get; }
        public double UnitPrice { get; }
    }
    
    /// <summary>
    /// Validated order line with value objects
    /// </summary>
    public record ValidatedOrderLine
    {
        internal ValidatedOrderLine(
            int productCode,
            ProductName productName,
            int quantity,
            Money unitPrice,
            Money lineTotal)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");
            
            ProductCode = productCode;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            LineTotal = lineTotal;
        }
        
        public int ProductCode { get; }
        public ProductName ProductName { get; }
        public int Quantity { get; }
        public Money UnitPrice { get; }
        public Money LineTotal { get; }
    }
}
