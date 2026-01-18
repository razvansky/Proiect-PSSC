using System;
using System.Threading.Tasks;
using Lucrarea1PSSC.clase.Workflow.Events;
using Lucrarea1PSSC.clase.Infrastructure.Messaging;
using Lucrarea1PSSC.api.Services.Delivery;
using Lucrarea1PSSC.api.DTOs.Delivery;
using Lucrarea1PSSC.clase.Workflow.ValueObjects;

namespace Lucrarea1PSSC.clase.Workflow.Delivery
{
    /// <summary>
    /// Delivery initiated event
    /// </summary>
    public class DeliveryInitiatedEvent : IMessage
    {
        public Guid MessageId { get; }
        public DateTime Timestamp { get; }
        public string MessageType => "DeliveryInitiated";

        public Guid OrderNumber { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public DateTime EstimatedDeliveryDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;

        public DeliveryInitiatedEvent(Guid orderNumber, string trackingNumber, string carrier, DateTime estimatedDeliveryDate, string customerName)
        {
            MessageId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            OrderNumber = orderNumber;
            TrackingNumber = trackingNumber;
            Carrier = carrier;
            EstimatedDeliveryDate = estimatedDeliveryDate;
            CustomerName = customerName;
        }
    }

    /// <summary>
    /// Delivery Initiation Workflow
    /// Listens to OrderPlacedEvent and initiates delivery
    /// </summary>
    public class DeliveryInitiationWorkflow
    {
        private readonly DeliveryApiClient? _deliveryClient;
        private readonly IMessageTopic<DeliveryInitiatedEvent>? _deliveryInitiatedTopic;

        public DeliveryInitiationWorkflow(DeliveryApiClient? deliveryClient = null, IMessageTopic<DeliveryInitiatedEvent>? deliveryInitiatedTopic = null)
        {
            _deliveryClient = deliveryClient;
            _deliveryInitiatedTopic = deliveryInitiatedTopic;
        }

        /// <summary>
        /// Process order placed event and initiate delivery
        /// </summary>
        public async Task<DeliveryResponse> InitiateDeliveryAsync(OrderPlacedEvent orderEvent)
        {
            Console.WriteLine($"[DELIVERY WORKFLOW] Starting delivery initiation for order {orderEvent.OrderNumber}...");

            await Task.Delay(300);

            DeliveryResponse deliveryResponse;

            if (_deliveryClient != null)
            {
                Console.WriteLine($"[DELIVERY WORKFLOW] Calling external delivery API...");

                var deliveryRequest = new DeliveryRequest
                {
                    OrderNumber = orderEvent.OrderNumber,
                    CustomerName = orderEvent.CustomerName,
                    DeliveryAddress = orderEvent.DeliveryAddress,
                    City = ExtractCity(orderEvent.DeliveryAddress),
                    PostalCode = "400000",
                    Country = "Romania",
                    Phone = "0721234567",
                    TotalAmount = orderEvent.TotalAmount,
                    TotalItems = orderEvent.TotalItems,
                    OrderDate = orderEvent.OrderDate,
                    Priority = orderEvent.TotalAmount > 1000 ? "Express" : "Standard",
                    Notes = $"Order {orderEvent.OrderNumber}"
                };

                deliveryResponse = await _deliveryClient.ScheduleDeliveryAsync(deliveryRequest);
            }
            else
            {
                Console.WriteLine($"[DELIVERY WORKFLOW] Using simulated delivery scheduling...");
                deliveryResponse = SimulateDeliveryScheduling(orderEvent);
            }

            if (deliveryResponse.Success)
            {
                Console.WriteLine($"[DELIVERY WORKFLOW] Delivery scheduled successfully");
                Console.WriteLine($"[DELIVERY WORKFLOW] Tracking Number: {deliveryResponse.TrackingNumber}");
                Console.WriteLine($"[DELIVERY WORKFLOW] Carrier: {deliveryResponse.Carrier}");
                Console.WriteLine($"[DELIVERY WORKFLOW] Estimated Delivery: {deliveryResponse.EstimatedDeliveryDate:yyyy-MM-dd}");

                if (_deliveryInitiatedTopic != null)
                {
                    var deliveryInitiatedEvent = new DeliveryInitiatedEvent(
                        orderEvent.OrderNumber,
                        deliveryResponse.TrackingNumber,
                        deliveryResponse.Carrier,
                        deliveryResponse.EstimatedDeliveryDate,
                        orderEvent.CustomerName);

                    await _deliveryInitiatedTopic.PublishAsync(deliveryInitiatedEvent);
                    Console.WriteLine($"[DELIVERY WORKFLOW] Published DeliveryInitiatedEvent to topic");
                }

                PrintDeliveryLabel(orderEvent, deliveryResponse);
            }
            else
            {
                Console.WriteLine($"[DELIVERY WORKFLOW] Delivery scheduling failed: {deliveryResponse.Message}");
            }

            return deliveryResponse;
        }

        private DeliveryResponse SimulateDeliveryScheduling(OrderPlacedEvent orderEvent)
        {
            var shipmentId = ShipmentId.Create();
            var trackingNumber = $"TRK-{DateTime.Now:yyyyMMdd}-{shipmentId.Value.ToString("N")[..6].ToUpperInvariant()}";
            var carrier = orderEvent.TotalAmount > 1000 ? "FAN Courier Express" : "Romanian Post";
            var estimatedDays = orderEvent.TotalAmount > 1000 ? 1 : 3;

            return new DeliveryResponse
            {
                Success = true,
                DeliveryId = shipmentId.Value.ToString(),
                TrackingNumber = trackingNumber,
                Carrier = carrier,
                EstimatedDeliveryDate = DateTime.Now.AddDays(estimatedDays),
                Status = "Pending",
                Message = "Delivery scheduled successfully"
            }; 
        }

        private string ExtractCity(string address)
        {
            if (address.Contains("Cluj")) return "Cluj-Napoca";
            if (address.Contains("Bucuresti") || address.Contains("Bucharest")) return "Bucuresti";
            if (address.Contains("Iasi")) return "Iasi";
            if (address.Contains("Timisoara")) return "Timisoara";
            return "Cluj-Napoca";
        }

        private void PrintDeliveryLabel(OrderPlacedEvent orderEvent, DeliveryResponse deliveryResponse)
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("              DELIVERY SHIPPING LABEL                  ");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();
            Console.WriteLine($"Tracking Number: {deliveryResponse.TrackingNumber}");
            Console.WriteLine($"Carrier:         {deliveryResponse.Carrier}");
            Console.WriteLine($"Order Number:    {orderEvent.OrderNumber}");
            Console.WriteLine();
            Console.WriteLine("SHIP TO:");
            Console.WriteLine($"  {orderEvent.CustomerName}");
            Console.WriteLine($"  {orderEvent.DeliveryAddress}");
            Console.WriteLine();
            Console.WriteLine($"Package Details:");
            Console.WriteLine($"  Items:     {orderEvent.TotalItems}");
            Console.WriteLine($"  Value:     {orderEvent.TotalAmount:F2} RON");
            Console.WriteLine($"  Priority:  {(orderEvent.TotalAmount > 1000 ? "EXPRESS" : "STANDARD")}");
            Console.WriteLine();
            Console.WriteLine($"Estimated Delivery: {deliveryResponse.EstimatedDeliveryDate:yyyy-MM-dd}");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();
        }
    }
}
