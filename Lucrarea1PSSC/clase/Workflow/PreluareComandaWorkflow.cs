using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.Infrastructure;
using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.clase.Workflow.Commands;
using Lucrarea1PSSC.clase.Workflow.ValueObjects;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.ClaseProduse;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.clase.Workflow
{
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

        public void RegisterOrder(ComandaAggregate order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository[order.ComandaId] = order;
        }

        public async Task<ComandaEvent.IComandaEvent> PreluareComandaAsync(Guid comandaId, string operatorName)
        {
            Console.WriteLine($"\n[PRELUARE WORKFLOW] Starting order pickup for {comandaId}...");

            var (cmdSuccess, command, cmdError) = PreluareComandaCommand.TryCreate(comandaId, operatorName);
            if (!cmdSuccess)
            {
                Console.WriteLine($"[PRELUARE WORKFLOW] Command validation failed: {cmdError}");
                return new ComandaEvent.ComandaPreluataFailedEvent(comandaId, cmdError!, DateTime.UtcNow);
            }

            if (!_orderRepository.TryGetValue(comandaId, out var order))
            {
                // Fallback: try DB
                if (_dbService != null)
                {
                    try
                    {
                        var dbOrder = await _dbService.GetOrderDetailsAsync(comandaId);
                        if (dbOrder != null)
                        {
                            var produse = new List<ProdusCos>();
                            foreach (var item in dbOrder.OrderItems)
                            {
                                produse.Add(new ProdusCos(
                                    new CodProdus(item.ProductCode),
                                    item.ProductName,
                                    new UnitQuantity((double)item.Quantity),
                                    new KilogramQuantity(1.0),
                                    new Price((double)item.UnitPrice)));
                            }

                            var mappedState = dbOrder.Status switch
                            {
                                "Placed" => StaraComanda.Plasata,
                                "InPregatire" => StaraComanda.InPregatire,
                                "Shipped" => StaraComanda.Expediata,
                                "Delivered" => StaraComanda.Livrata,
                                "Cancelled" => StaraComanda.Anulata,
                                _ => StaraComanda.Plasata
                            };

                            order = ComandaAggregate.CreateFromDatabase(
                                dbOrder.OrderNumber,
                                dbOrder.Customer?.Name ?? "Unknown",
                                dbOrder.DeliveryAddress ?? "Unknown",
                                produse,
                                Money.FromDouble((double)dbOrder.Total, "RON"),
                                dbOrder.OrderDate,
                                mappedState);

                            RegisterOrder(order);
                            Console.WriteLine($"[PRELUARE WORKFLOW] Loaded order {comandaId} from database and registered in repository");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PRELUARE WORKFLOW] Database error: {ex.Message}");
                    }
                }

                if (order == null)
                {
                    return new ComandaEvent.ComandaPreluataFailedEvent(
                        comandaId,
                        "Comanda nu a fost gasita in sistem",
                        DateTime.UtcNow);
                }
            }

            var validation = ValidateOrderForPickup(order);
            if (!validation.IsValid)
            {
                Console.WriteLine($"[PRELUARE WORKFLOW] Validation failed: {string.Join(", ", validation.Errors)}");

                EventBus.Publish(new ComandaEvent.ValidareComandaFailedEvent(
                    comandaId,
                    validation.Errors,
                    DateTime.UtcNow));

                return new ComandaEvent.ComandaPreluataFailedEvent(
                    comandaId,
                    string.Join("; ", validation.Errors),
                    DateTime.UtcNow);
            }

            var transition = order.StartPreparation(operatorName);
            if (transition.IsFailure)
            {
                var err = transition.GetErrorOrThrow();
                Console.WriteLine($"[PRELUARE WORKFLOW] State transition failed: {err}");
                return new ComandaEvent.ComandaPreluataFailedEvent(comandaId, err.Message, DateTime.UtcNow);
            }

            Console.WriteLine($"[PRELUARE WORKFLOW] Order state changed to: {order.Stare}");

            var estimatedDuration = CalculateEstimatedPreparationTime(order);

            var successEvent = new ComandaEvent.ComandaPreluataSuccessEvent(
                order.ComandaId,
                order.NumeClient,
                operatorName,
                order.Total.ToDouble(),
                order.Produse.Count,
                command!.PickupTime);

            EventBus.Publish(successEvent);

            var preparationEvent = new ComandaEvent.ComandaInPregatireEvent(
                order.ComandaId,
                operatorName,
                DateTime.UtcNow,
                estimatedDuration);

            EventBus.Publish(preparationEvent);

            if (_dbService != null)
            {
                await UpdateOrderStatusInDatabaseAsync(comandaId, "InPregatire");
            }

            Console.WriteLine($"[PRELUARE WORKFLOW] Order pickup completed successfully");
            PrintPickupReceipt(order, operatorName, estimatedDuration);

            return successEvent;
        }

        public ComandaEvent.IComandaEvent PreluareComanda(Guid comandaId, string operatorName)
            => PreluareComandaAsync(comandaId, operatorName).GetAwaiter().GetResult();

        private (bool IsValid, string[] Errors) ValidateOrderForPickup(ComandaAggregate order)
        {
            var errors = new List<string>();

            if (order.Stare != StaraComanda.Plasata)
                errors.Add($"Comanda trebuie sa fie in starea 'Plasata', dar este in starea '{order.Stare}'");

            if (order.Produse == null || order.Produse.Count == 0)
                errors.Add("Comanda nu contine produse");

            if (order.Total.Amount <= 0)
                errors.Add("Totalul comenzii trebuie sa fie pozitiv");

            if (string.IsNullOrWhiteSpace(order.NumeClient))
                errors.Add("Comanda nu are un client asociat");

            if (order.AdresaLivrare == null || string.IsNullOrWhiteSpace(order.AdresaLivrare.adress))
                errors.Add("Comanda nu are o adresa de livrare valida");

            if ((DateTime.UtcNow - order.DataPlasare).TotalDays > 7)
                errors.Add("Comanda este mai veche de 7 zile si necesita verificare suplimentara");

            return (errors.Count == 0, errors.ToArray());
        }

        private TimeSpan CalculateEstimatedPreparationTime(ComandaAggregate order) => order.EstimatedPreparationTime;

        private async Task UpdateOrderStatusInDatabaseAsync(Guid comandaId, string status)
        {
            try
            {
                Console.WriteLine($"[PRELUARE WORKFLOW] Updating order {comandaId} status to {status} in database...");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PRELUARE WORKFLOW] Failed to update database: {ex.Message}");
            }
        }

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
            Console.WriteLine($"? Total:                              {order.Total.Amount,10:F2} {order.Total.Currency} ?");
            Console.WriteLine($"? Nr. Produse:                                      {order.Produse.Count,4} ?");
            Console.WriteLine($"? Est. Pregatire:                          {estimatedDuration.TotalMinutes,7:F0} min ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine($"? Stare:        {order.Stare,-40} ?");
            Console.WriteLine("????????????????????????????????????????????????????????????");
            Console.WriteLine("? Adresa Livrare:                                          ?");
            Console.WriteLine($"?   {order.AdresaLivrare.adress,-54} ?");
            Console.WriteLine("????????????????????????????????????????????????????????????\n");
        }

        public ComandaEvent.IComandaEvent FinalizeazaPregatire(Guid comandaId, string operatorName, bool isReadyForShipment = true)
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
                order.Total.ToDouble(),
                order.Produse.Count,
                DateTime.UtcNow);
        }

        public ComandaAggregate? GetOrder(Guid comandaId)
        {
            _orderRepository.TryGetValue(comandaId, out var order);
            return order;
        }

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
