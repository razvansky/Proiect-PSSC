using System;
using System.Threading.Tasks;
using Lucrarea1PSSC.clase.Infrastructure.Messaging;
using Lucrarea1PSSC.clase.Workflow.Events;
using Lucrarea1PSSC.clase.Workflow.Invoice;
using Lucrarea1PSSC.clase.Workflow.Delivery;
using Lucrarea1PSSC.api.Services.Delivery;

namespace Lucrarea1PSSC.clase.Workflow.Orchestration
{
    /// <summary>
    /// Orchestrates the order processing workflow
    /// Implements event-driven architecture with message queues and topics
    /// </summary>
    public class OrderProcessingOrchestrator
    {
        // Message infrastructure
        private readonly IMessageTopic<OrderPlacedEvent> _orderPlacedTopic;
        private readonly IMessageTopic<InvoiceGeneratedEvent> _invoiceGeneratedTopic;
        private readonly IMessageTopic<DeliveryInitiatedEvent> _deliveryInitiatedTopic;

        // Workflows
        private readonly InvoiceGenerationWorkflow _invoiceWorkflow;
        private readonly DeliveryInitiationWorkflow _deliveryWorkflow;

        public OrderProcessingOrchestrator(DeliveryApiClient? deliveryClient = null)
        {
            Console.WriteLine("\n[ORCHESTRATOR] Initializing Order Processing Orchestrator...");

            // Create topics (1-to-many communication)
            _orderPlacedTopic = new InMemoryMessageTopic<OrderPlacedEvent>("order-placed");
            _invoiceGeneratedTopic = new InMemoryMessageTopic<InvoiceGeneratedEvent>("invoice-generated");
            _deliveryInitiatedTopic = new InMemoryMessageTopic<DeliveryInitiatedEvent>("delivery-initiated");

            // Create workflows
            _invoiceWorkflow = new InvoiceGenerationWorkflow(_invoiceGeneratedTopic);
            _deliveryWorkflow = new DeliveryInitiationWorkflow(deliveryClient, _deliveryInitiatedTopic);

            // Subscribe workflows to OrderPlacedEvent
            Console.WriteLine("[ORCHESTRATOR] Setting up event subscriptions...");
            
            _orderPlacedTopic.Subscribe(async (orderEvent) =>
            {
                Console.WriteLine($"\n[ORCHESTRATOR] Processing OrderPlacedEvent for order {orderEvent.OrderNumber}");
                Console.WriteLine($"[ORCHESTRATOR] Triggering Invoice Generation workflow...");
                await _invoiceWorkflow.GenerateInvoiceAsync(orderEvent);
            });

            _orderPlacedTopic.Subscribe(async (orderEvent) =>
            {
                Console.WriteLine($"\n[ORCHESTRATOR] Processing OrderPlacedEvent for order {orderEvent.OrderNumber}");
                Console.WriteLine($"[ORCHESTRATOR] Triggering Delivery Initiation workflow...");
                await _deliveryWorkflow.InitiateDeliveryAsync(orderEvent);
            });

            // Subscribe to InvoiceGeneratedEvent (example of chaining)
            _invoiceGeneratedTopic.Subscribe(async (invoiceEvent) =>
            {
                Console.WriteLine($"\n[ORCHESTRATOR] Invoice {invoiceEvent.InvoiceNumber} generated");
                Console.WriteLine($"[ORCHESTRATOR] Could send email to {invoiceEvent.CustomerEmail}...");
                await Task.CompletedTask;
            });

            // Subscribe to DeliveryInitiatedEvent
            _deliveryInitiatedTopic.Subscribe(async (deliveryEvent) =>
            {
                Console.WriteLine($"\n[ORCHESTRATOR] Delivery {deliveryEvent.TrackingNumber} initiated for {deliveryEvent.CustomerName}");
                Console.WriteLine($"[ORCHESTRATOR] Could send tracking SMS/email...");
                await Task.CompletedTask;
            });

            Console.WriteLine($"[ORCHESTRATOR] ? Orchestrator initialized with {_orderPlacedTopic.SubscriberCount} subscribers to OrderPlacedEvent");
            Console.WriteLine($"[ORCHESTRATOR] ? Ready to process orders\n");
        }

        /// <summary>
        /// Process an order - emits OrderPlacedEvent
        /// </summary>
        public async Task ProcessOrderAsync(OrderPlacedEvent orderEvent)
        {
            Console.WriteLine("\n?????????????????????????????????????????????????????????????????");
            Console.WriteLine("?         ORDER PROCESSING STARTED                              ?");
            Console.WriteLine("?????????????????????????????????????????????????????????????????");
            Console.WriteLine($"Order:    {orderEvent.OrderNumber}");
            Console.WriteLine($"Customer: {orderEvent.CustomerName}");
            Console.WriteLine($"Total:    {orderEvent.TotalAmount:C} RON");
            Console.WriteLine($"Items:    {orderEvent.TotalItems}");
            Console.WriteLine("???????????????????????????????????????????????????????????????");

            // Publish to topic - all subscribers will process this event
            await _orderPlacedTopic.PublishAsync(orderEvent);

            Console.WriteLine("\n[ORCHESTRATOR] ? Order processing complete - all workflows triggered");
            Console.WriteLine("???????????????????????????????????????????????????????????????\n");
        }

        /// <summary>
        /// Get orchestrator status
        /// </summary>
        public string GetStatus()
        {
            return $"OrderPlaced subscribers: {_orderPlacedTopic.SubscriberCount}, " +
                   $"InvoiceGenerated subscribers: {_invoiceGeneratedTopic.SubscriberCount}, " +
                   $"DeliveryInitiated subscribers: {_deliveryInitiatedTopic.SubscriberCount}";
        }
    }

    /// <summary>
    /// Demonstrates 1-to-1 queue communication
    /// </summary>
    public class SimpleQueueExample
    {
        public static async Task DemonstrateQueue()
        {
            Console.WriteLine("\n?????????????????????????????????????????????????????????????????");
            Console.WriteLine("?    DEMONSTRATION: 1-to-1 QUEUE COMMUNICATION                  ?");
            Console.WriteLine("?????????????????????????????????????????????????????????????????\n");

            var queue = new InMemoryMessageQueue<OrderPlacedEvent>("demo-queue");

            // Producer
            Console.WriteLine("[PRODUCER] Sending messages to queue...");
            for (int i = 1; i <= 3; i++)
            {
                var orderEvent = new OrderPlacedEvent
                {
                    OrderNumber = Guid.NewGuid(),
                    CustomerName = $"Customer {i}",
                    CustomerEmail = $"customer{i}@example.com",
                    TotalAmount = 100 * i,
                    TotalItems = i
                };
                await queue.PublishAsync(orderEvent);
            }

            Console.WriteLine($"\n[QUEUE STATUS] Messages in queue: {queue.Count}");

            // Consumer
            Console.WriteLine("\n[CONSUMER] Consuming messages from queue...");
            while (queue.Count > 0)
            {
                var message = await queue.ConsumeAsync();
                Console.WriteLine($"[CONSUMER] Processing: {message}");
                await Task.Delay(500);
            }

            Console.WriteLine("\n[QUEUE STATUS] Queue is now empty");
            Console.WriteLine("???????????????????????????????????????????????????????????????\n");
        }
    }

    /// <summary>
    /// Demonstrates 1-to-many topic communication
    /// </summary>
    public class TopicExample
    {
        public static async Task DemonstrateTopic()
        {
            Console.WriteLine("\n?????????????????????????????????????????????????????????????????");
            Console.WriteLine("?    DEMONSTRATION: 1-to-MANY TOPIC COMMUNICATION               ?");
            Console.WriteLine("?????????????????????????????????????????????????????????????????\n");

            var topic = new InMemoryMessageTopic<OrderPlacedEvent>("demo-topic");

            // Subscribe multiple handlers
            topic.Subscribe(async (order) =>
            {
                Console.WriteLine($"[SUBSCRIBER 1 - INVOICE] Processing order {order.OrderNumber}");
                await Task.Delay(200);
                Console.WriteLine($"[SUBSCRIBER 1 - INVOICE] ? Invoice generated");
            });

            topic.Subscribe(async (order) =>
            {
                Console.WriteLine($"[SUBSCRIBER 2 - DELIVERY] Processing order {order.OrderNumber}");
                await Task.Delay(150);
                Console.WriteLine($"[SUBSCRIBER 2 - DELIVERY] ? Delivery scheduled");
            });

            topic.Subscribe(async (order) =>
            {
                Console.WriteLine($"[SUBSCRIBER 3 - EMAIL] Processing order {order.OrderNumber}");
                await Task.Delay(100);
                Console.WriteLine($"[SUBSCRIBER 3 - EMAIL] ? Confirmation email sent");
            });

            Console.WriteLine($"\n[TOPIC STATUS] Total subscribers: {topic.SubscriberCount}\n");

            // Publish event - all subscribers receive it
            var orderEvent = new OrderPlacedEvent
            {
                OrderNumber = Guid.NewGuid(),
                CustomerName = "Test Customer",
                CustomerEmail = "test@example.com",
                TotalAmount = 1500,
                TotalItems = 3
            };

            await topic.PublishAsync(orderEvent);

            Console.WriteLine("\n[TOPIC] All subscribers have processed the event");
            Console.WriteLine("???????????????????????????????????????????????????????????????\n");
        }
    }
}
