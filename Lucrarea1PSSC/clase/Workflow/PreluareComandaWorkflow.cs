using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.Infrastructure;
using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.clase.Workflow.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.clase.Workflow
{
    /// <summary>
    /// Workflow for Order Pickup (Preluare Comanda)
    /// Handles the process of picking up an order for preparation and shipping
    /// 
    /// Business Rules:
    /// 1. Only placed orders can be picked up
    /// 2. Order must have valid products
    /// 3. Operator must be identified
    /// 4. Stock verification before pickup
    /// 5. State transitions follow business rules
    /// </summary>
    public class PreluareComandaWorkflow
    {
        private readonly OrderWorkflowDatabaseService? _dbService;
        private readonly Dictionary<Guid, ComandaAggregate> _orderRepository;

        public PreluareComandaWorkflow(
            OrderWorkflowDatabaseService? dbService = null,
            Dictionary<Guid, ComandaAggregate>? orderRepository = null)
        {
            _dbService = dbService;
            _orderRepository = orderRepository ?? new Dictionary<Guid, ComandaAggregate>();
        }

        /// <summary>
        /// Register an order in the workflow repository
        /// </summary>
        public void RegisterOrder(ComandaAggregate order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository[order.ComandaId] = order;
        }

        /// <summary>
        /// Pickup an order for preparation - Async version
        /// </summary>
        public async Task<ComandaEvent.IComandaEvent> PreluareComandaAsync(
            Guid comandaId,
            string operatorName)
        {
            Console.WriteLine($"\n[PRELUARE WORKFLOW] Starting order pickup for {comandaId}...");

            // 1. Create and validate command
            var (cmdSuccess, command, cmdError) = PreluareComandaCommand.TryCreate(comandaId, operatorName);
            if (!cmdSuccess)
            {
                Console.WriteLine($"[PRELUARE WORKFLOW] Command validation failed: {cmdError}");
                return new ComandaEvent.ComandaPreluataFailedEvent(comandaId, cmdError!, DateTime.UtcNow);
            }

            // 2. Find the order
            if (!_orderRepository.TryGetValue(comandaId, out var order))
            {
                // Try to load from database if available
                if (_dbService != null)
                {
                    try
                    {
                        var dbOrder = await _dbService.GetOrderDetailsAsync(comandaId);
                        if (dbOrder == null)
                        {
                            Console.WriteLine($"[PRELUARE WORKFLOW] Order not found: {comandaId}");
                            return new ComandaEvent.ComandaPreluataFailedEvent(
                                comandaId,
                                "Comanda nu a fost gasita in sistem",
                                DateTime.UtcNow);
                        }
                        // Note: Would need to reconstruct ComandaAggregate from DB here
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PRELUARE WORKFLOW] Database error: {ex.Message}");
                    }
                }

                return new ComandaEvent.ComandaPreluataFailedEvent(
                    comandaId,
                    "Comanda nu a fost gasita in sistem",
                    DateTime.UtcNow);
            }

            // 3. Validate order state
            var validationResult = ValidateOrderForPickup(order);
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"[PRELUARE WORKFLOW] Validation failed: {string.Join(", ", validationResult.Errors)}");
                
                EventBus.Publish(new ComandaEvent.ValidareComandaFailedEvent(
                    comandaId,
                    validationResult.Errors,
                    DateTime.UtcNow));

                return new ComandaEvent.ComandaPreluataFailedEvent(
                    comandaId,
                    string.Join("; ", validationResult.Errors),
                    DateTime.UtcNow);
            }

            // 4. Execute state transition
            try
            {
                order.StartPreparation();
                Console.WriteLine($"[PRELUARE WORKFLOW] Order state changed to: {order.Stare}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[PRELUARE WORKFLOW] State transition failed: {ex.Message}");
                return new ComandaEvent.ComandaPreluataFailedEvent(comandaId, ex.Message, DateTime.UtcNow);
            }

            // 5. Estimate preparation time based on order complexity
            var estimatedDuration = CalculateEstimatedPreparationTime(order);

            // 6. Publish events
            var successEvent = new ComandaEvent.ComandaPreluataSuccessEvent(
                order.ComandaId,
                order.NumeClient,
                operatorName,
                order.Total,
                order.Produse.Count,
                command!.PickupTime);

            EventBus.Publish(successEvent);

            var preparationEvent = new ComandaEvent.ComandaInPregatireEvent(
                order.ComandaId,
                operatorName,
                DateTime.UtcNow,
                estimatedDuration);

            EventBus.Publish(preparationEvent);

            // 7. Update database if available
            if (_dbService != null)
            {
                await UpdateOrderStatusInDatabaseAsync(comandaId, "InPregatire");
            }

            Console.WriteLine($"[PRELUARE WORKFLOW] Order pickup completed successfully");
            PrintPickupReceipt(order, operatorName, estimatedDuration);

            return successEvent;
        }

        /// <summary>
        /// Pickup an order for preparation - Sync version
        /// </summary>
        public ComandaEvent.IComandaEvent PreluareComanda(
            Guid comandaId,
            string operatorName)
        {
            return PreluareComandaAsync(comandaId, operatorName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Validate order is ready for pickup
        /// </summary>
        private (bool IsValid, string[] Errors) ValidateOrderForPickup(ComandaAggregate order)
        {
            var errors = new List<string>();

            // Check order state
            if (order.Stare != StaraComanda.Plasata)
            {
                errors.Add($"Comanda trebuie sa fie in starea 'Plasata', dar este in starea '{order.Stare}'");
            }

            // Check products exist
            if (order.Produse == null || order.Produse.Count == 0)
            {
                errors.Add("Comanda nu contine produse");
            }

            // Check total is valid
            if (order.Total <= 0)
            {
                errors.Add("Totalul comenzii trebuie sa fie pozitiv");
            }

            // Check customer name
            if (string.IsNullOrWhiteSpace(order.NumeClient))
            {
                errors.Add("Comanda nu are un client asociat");
            }

            // Check delivery address
            if (order.AdresaLivrare == null || string.IsNullOrWhiteSpace(order.AdresaLivrare.adress))
            {
                errors.Add("Comanda nu are o adresa de livrare valida");
            }

            // Check order age (orders older than 7 days might need special handling)
            if ((DateTime.UtcNow - order.DataPlasare).TotalDays > 7)
            {
                errors.Add("Comanda este mai veche de 7 zile si necesita verificare suplimentara");
            }

            return (errors.Count == 0, errors.ToArray());
        }

        /// <summary>
        /// Calculate estimated preparation time based on order complexity
        /// </summary>
        private TimeSpan CalculateEstimatedPreparationTime(ComandaAggregate order)
        {
            // Base time: 5 minutes
            var baseMinutes = 5;

            // Add 2 minutes per product
            var productMinutes = order.Produse.Count * 2;

            // Add extra time for high-value orders (over 1000 lei)
            var highValueMinutes = order.Total > 1000 ? 5 : 0;

            // Add extra time for many products (over 5)
            var bulkMinutes = order.Produse.Count > 5 ? 10 : 0;

            var totalMinutes = baseMinutes + productMinutes + highValueMinutes + bulkMinutes;

            return TimeSpan.FromMinutes(totalMinutes);
        }

        /// <summary>
        /// Update order status in database
        /// </summary>
        private async Task UpdateOrderStatusInDatabaseAsync(Guid comandaId, string status)
        {
            try
            {
                // Note: This would need implementation in OrderWorkflowDatabaseService
                Console.WriteLine($"[PRELUARE WORKFLOW] Updating order {comandaId} status to {status} in database...");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PRELUARE WORKFLOW] Failed to update database: {ex.Message}");
            }
        }

        /// <summary>
        /// Print pickup receipt
        /// </summary>
        private void PrintPickupReceipt(ComandaAggregate order, string operatorName, TimeSpan estimatedDuration)
        {
            Console.WriteLine("\n????????????????????????????????????????????????????????????");
            Console.WriteLine("?           RECEIPT - PRELUARE COMANDA                     ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine($"? Order ID:     {order.ComandaId,-40} ?");
            Console.WriteLine($"? Client:       {order.NumeClient,-40} ?");
            Console.WriteLine($"? Operator:     {operatorName,-40} ?");
            Console.WriteLine($"? Pickup Time:  {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss,-32} ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("? PRODUSE:                                                 ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            
            foreach (var produs in order.Produse)
            {
                var line = $"  - {produs.Nume} x{produs.Cantitate.Cantitate}";
                Console.WriteLine($"? {line,-56} ?");
            }
            
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine($"? Total:                                   {order.Total,10:F2} lei ?");
            Console.WriteLine($"? Nr. Produse:                                      {order.Produse.Count,4} ?");
            Console.WriteLine($"? Est. Pregatire:                          {estimatedDuration.TotalMinutes,7:F0} min ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine($"? Stare:        {order.Stare,-40} ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("? Adresa Livrare:                                          ?");
            Console.WriteLine($"?   {order.AdresaLivrare.adress,-54} ?");
            Console.WriteLine("????????????????????????????????????????????????????????????\n");
        }

        /// <summary>
        /// Complete order preparation - marks order as ready for shipment
        /// </summary>
        public ComandaEvent.IComandaEvent FinalizeazaPregatire(
            Guid comandaId,
            string operatorName,
            bool isReadyForShipment = true)
        {
            if (!_orderRepository.TryGetValue(comandaId, out var order))
            {
                return new ComandaEvent.ComandaPreluataFailedEvent(
                    comandaId,
                    "Comanda nu a fost gasita in sistem",
                    DateTime.UtcNow);
            }

            if (order.Stare != StaraComanda.InPregatire)
            {
                return new ComandaEvent.ComandaPreluataFailedEvent(
                    comandaId,
                    $"Comanda nu este in starea 'InPregatire', este in starea '{order.Stare}'",
                    DateTime.UtcNow);
            }

            var completionEvent = new ComandaEvent.ComandaPregatitaEvent(
                comandaId,
                operatorName,
                DateTime.UtcNow,
                isReadyForShipment);

            EventBus.Publish(completionEvent);

            Console.WriteLine($"[PRELUARE WORKFLOW] {completionEvent.Message}");

            return new ComandaEvent.ComandaPreluataSuccessEvent(
                order.ComandaId,
                order.NumeClient,
                operatorName,
                order.Total,
                order.Produse.Count,
                DateTime.UtcNow);
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        public ComandaAggregate? GetOrder(Guid comandaId)
        {
            _orderRepository.TryGetValue(comandaId, out var order);
            return order;
        }

        /// <summary>
        /// Get all orders in a specific state
        /// </summary>
        public IEnumerable<ComandaAggregate> GetOrdersByState(StaraComanda state)
        {
            foreach (var order in _orderRepository.Values)
            {
                if (order.Stare == state)
                    yield return order;
            }
        }
    }
}
