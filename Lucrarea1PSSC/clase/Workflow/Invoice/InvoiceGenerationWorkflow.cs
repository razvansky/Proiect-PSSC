using System;
using System.Text;
using System.Threading.Tasks;
using Lucrarea1PSSC.clase.Workflow.Events;
using Lucrarea1PSSC.clase.Infrastructure.Messaging;

namespace Lucrarea1PSSC.clase.Workflow.Invoice
{
    /// <summary>
    /// Invoice model
    /// </summary>
    public class InvoiceModel
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid OrderNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        
        // Customer details
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        
        // Financial details
        public decimal Subtotal { get; set; }
        public decimal TaxRate { get; set; } = 0.19m; // 19% VAT
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Items
        public List<InvoiceLineItem> LineItems { get; set; } = new();
        
        // Status
        public string Status { get; set; } = "Generated";
        public DateTime CreatedAt { get; set; }
    }

    public class InvoiceLineItem
    {
        public int ProductCode { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// Invoice generated event
    /// </summary>
    public class InvoiceGeneratedEvent : IMessage
    {
        public Guid MessageId { get; }
        public DateTime Timestamp { get; }
        public string MessageType => "InvoiceGenerated";
        
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid OrderNumber { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        public InvoiceGeneratedEvent(Guid invoiceId, string invoiceNumber, Guid orderNumber, string customerEmail, decimal totalAmount)
        {
            MessageId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            InvoiceId = invoiceId;
            InvoiceNumber = invoiceNumber;
            OrderNumber = orderNumber;
            CustomerEmail = customerEmail;
            TotalAmount = totalAmount;
        }
    }

    /// <summary>
    /// Invoice Generation Workflow
    /// Listens to OrderPlacedEvent and generates invoices
    /// </summary>
    public class InvoiceGenerationWorkflow
    {
        private readonly IMessageTopic<InvoiceGeneratedEvent>? _invoiceGeneratedTopic;
        private static int _invoiceCounter = 1000;

        public InvoiceGenerationWorkflow(IMessageTopic<InvoiceGeneratedEvent>? invoiceGeneratedTopic = null)
        {
            _invoiceGeneratedTopic = invoiceGeneratedTopic;
        }

        /// <summary>
        /// Process order placed event and generate invoice
        /// </summary>
        public async Task<InvoiceModel> GenerateInvoiceAsync(OrderPlacedEvent orderEvent)
        {
            Console.WriteLine($"[INVOICE WORKFLOW] Starting invoice generation for order {orderEvent.OrderNumber}...");

            try
            {
                // Simulate processing time
                await Task.Delay(500);

                // Generate invoice number
                var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{Interlocked.Increment(ref _invoiceCounter):D6}";

                // Calculate tax
                var subtotal = orderEvent.TotalAmount;
                var taxRate = 0.19m; // 19% VAT (Romanian standard rate)
                var taxAmount = Math.Round(subtotal * taxRate, 2);
                var totalWithTax = subtotal + taxAmount;

                // Create invoice model
                var invoice = new InvoiceModel
                {
                    InvoiceId = Guid.NewGuid(),
                    InvoiceNumber = invoiceNumber,
                    OrderNumber = orderEvent.OrderNumber,
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    CustomerName = orderEvent.CustomerName,
                    CustomerEmail = orderEvent.CustomerEmail,
                    BillingAddress = orderEvent.DeliveryAddress,
                    Subtotal = subtotal,
                    TaxRate = taxRate,
                    TaxAmount = taxAmount,
                    TotalAmount = totalWithTax,
                    Status = "Generated",
                    CreatedAt = DateTime.UtcNow
                };

                // Add line items
                foreach (var item in orderEvent.Items)
                {
                    invoice.LineItems.Add(new InvoiceLineItem
                    {
                        ProductCode = item.ProductCode,
                        Description = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    });
                }

                Console.WriteLine($"[INVOICE WORKFLOW] ? Invoice generated: {invoiceNumber}");
                Console.WriteLine($"[INVOICE WORKFLOW] Subtotal: {subtotal:C} RON");
                Console.WriteLine($"[INVOICE WORKFLOW] VAT (19%): {taxAmount:C} RON");
                Console.WriteLine($"[INVOICE WORKFLOW] Total: {totalWithTax:C} RON");

                // Print invoice details
                PrintInvoice(invoice);

                // Publish invoice generated event
                if (_invoiceGeneratedTopic != null)
                {
                    var invoiceGeneratedEvent = new InvoiceGeneratedEvent(
                        invoice.InvoiceId,
                        invoice.InvoiceNumber,
                        invoice.OrderNumber,
                        invoice.CustomerEmail,
                        invoice.TotalAmount
                    );

                    await _invoiceGeneratedTopic.PublishAsync(invoiceGeneratedEvent);
                    Console.WriteLine($"[INVOICE WORKFLOW] Published InvoiceGeneratedEvent to topic");
                }

                return invoice;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INVOICE WORKFLOW] ? Error generating invoice: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Print invoice to console (simulates PDF generation)
        /// </summary>
        private void PrintInvoice(InvoiceModel invoice)
        {
            var sb = new StringBuilder();
            sb.AppendLine("\n?????????????????????????????????????????????????????????");
            sb.AppendLine("?               E-COMMERCE INVOICE                      ?");
            sb.AppendLine("?????????????????????????????????????????????????????????");
            sb.AppendLine();
            sb.AppendLine($"Invoice Number: {invoice.InvoiceNumber}");
            sb.AppendLine($"Order Number:   {invoice.OrderNumber}");
            sb.AppendLine($"Invoice Date:   {invoice.InvoiceDate:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Due Date:       {invoice.DueDate:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("BILL TO:");
            sb.AppendLine($"  {invoice.CustomerName}");
            sb.AppendLine($"  {invoice.BillingAddress}");
            sb.AppendLine($"  Email: {invoice.CustomerEmail}");
            sb.AppendLine();
            sb.AppendLine("ITEMS:");
            sb.AppendLine("?????????????????????????????????????????????????????????");
            sb.AppendLine($"{"Description",-30} {"Qty",5} {"Price",10} {"Total",10}");
            sb.AppendLine("?????????????????????????????????????????????????????????");
            
            foreach (var item in invoice.LineItems)
            {
                sb.AppendLine($"{item.Description,-30} {item.Quantity,5} {item.UnitPrice,10:F2} {item.LineTotal,10:F2}");
            }
            
            sb.AppendLine("?????????????????????????????????????????????????????????");
            sb.AppendLine($"{"Subtotal:",-46} {invoice.Subtotal,10:F2} RON");
            sb.AppendLine($"{"VAT (19%):",-46} {invoice.TaxAmount,10:F2} RON");
            sb.AppendLine("?????????????????????????????????????????????????????????");
            sb.AppendLine($"{"TOTAL:",-46} {invoice.TotalAmount,10:F2} RON");
            sb.AppendLine("?????????????????????????????????????????????????????????");
            sb.AppendLine();
            sb.AppendLine("Thank you for your business!");
            sb.AppendLine();

            Console.WriteLine(sb.ToString());
        }
    }
}
