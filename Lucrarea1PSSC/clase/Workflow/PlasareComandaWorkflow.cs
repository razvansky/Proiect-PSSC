using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.Infrastructure.Database;
using Lucrarea1PSSC.clase.Workflow.Events;
using Lucrarea1PSSC.clase.Workflow.Orchestration;
using Lucrarea1PSSC.clase.Infrastructure.Messaging;
using Lucrarea1PSSC.clase.Infrastructure;
using Lucrarea1PSSC.clase.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static partial class ComandaEvent
{
    public interface IComandaEvent { }

    public record ComandaPlasataSuccessEvent : IComandaEvent
    {
        internal ComandaPlasataSuccessEvent(string numePers, double total, int numarProduse, Guid comandaId, string? trackingNumber = null)
        {
            NumePersoana = numePers;
            TotalComanda = total;
            NumarProduse = numarProduse;
            ComandaId = comandaId;
            TrackingNumber = trackingNumber;
        }

        public string NumePersoana { get; }
        public double TotalComanda { get; }
        public int NumarProduse { get; }
        public Guid ComandaId { get; }
        public string? TrackingNumber { get; }
        public string Message => TrackingNumber != null 
            ? $"Comanda a fost plasata cu succes pentru {NumePersoana}. Total: {TotalComanda} lei, Produse: {NumarProduse}. Tracking: {TrackingNumber}"
            : $"Comanda a fost plasata cu succes pentru {NumePersoana}. Total: {TotalComanda} lei, Produse: {NumarProduse}";
    }

    public record ComandaPlasataFailedEvent : IComandaEvent
    {
        internal ComandaPlasataFailedEvent(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; }
        public string Message => $"Comanda nu a putut fi plasata: {Reason}";
    }
}

public class PlasareComandaWorkflow
{
    private readonly OrderWorkflowDatabaseService? _dbService;
    private readonly OrderProcessingOrchestrator? _orchestrator;
    private readonly IMessageBus? _messageBus;
    private readonly Lucrarea1PSSC.clase.Workflow.PreluareComandaWorkflow? _preluareWorkflow;

    public PlasareComandaWorkflow(
        OrderWorkflowDatabaseService? dbService = null,
        OrderProcessingOrchestrator? orchestrator = null,
        IMessageBus? messageBus = null,
        Lucrarea1PSSC.clase.Workflow.PreluareComandaWorkflow? preluareWorkflow = null)
    {
        _dbService = dbService;
        _orchestrator = orchestrator;
        _messageBus = messageBus;
        _preluareWorkflow = preluareWorkflow;
    }

    public async Task<ComandaEvent.IComandaEvent> PlaseazaComandaAsync(
        Persoana persoana, 
        CosDeCumparaturi cos,
        List<Produs> produse)
    {
        // Validare date intrare
        if (persoana == null || cos == null)
            return new ComandaEvent.ComandaPlasataFailedEvent("Datele de intrare nu sunt valide.");

        // Verificare adresa livrare
        if (persoana.Adress.adress.Length < 5)
            return new ComandaEvent.ComandaPlasataFailedEvent("Adresa de livrare invalida.");

        // Verificare stare cos
        var stareCos = cos.GetStareCos();

        return stareCos switch
        {
            UnvalidatedCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul este invalid, generati un cos nou"),
            EmptyCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul este gol, adaugati produse in cos"),
            ValidatedCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul nu este platit, platiti cosul inainte de a plasa comanda"),
            PayedCos => await ProcessareComandaAsync(persoana, cos, produse),
            _ => new ComandaEvent.ComandaPlasataFailedEvent("Stare cos necunoscuta.")
        };
    }

    private async Task<ComandaEvent.IComandaEvent> ProcessareComandaAsync(
        Persoana persoana, 
        CosDeCumparaturi cos,
        List<Produs> produse)
    {
        try
        {
            var totalComanda = cos.TotalCos();
            var numarProduse = cos.GetProduseCos().Count;

            var orderNumber = Guid.NewGuid();

            // Save to database if service is available
            if (_dbService != null)
            {
                Console.WriteLine("[WORKFLOW] Saving order to database...");
                var (success, order, error) = await _dbService.PlaceOrderInDatabaseAsync(
                    orderNumber,
                    persoana,
                    cos,
                    produse
                );

                if (!success)
                {
                    return new ComandaEvent.ComandaPlasataFailedEvent(
                        error ?? "Failed to save order to database"
                    );
                }

                Console.WriteLine("[WORKFLOW] Order saved to database successfully");
                orderNumber = order!.OrderNumber;
            }

            // Register the order so PreluareComanda endpoints can find it
            if (_preluareWorkflow != null)
            {
                var aggResult = ComandaAggregate.CreateFromPaidCartWithId(orderNumber, persoana, cos);
                if (aggResult is Result<ComandaAggregate>.Success s)
                {
                    _preluareWorkflow.RegisterOrder(s.Value);
                }
            }

            // Publish on bus (used by Billing/Shipping contexts once added)
            if (_messageBus != null)
            {
                await _messageBus.PublishAsync(new OrderPlacedIntegrationMessage(
                    orderNumber,
                    persoana.Nume.Name,
                    persoana.Email.email,
                    persoana.Adress.adress,
                    (decimal)totalComanda,
                    numarProduse));
            }

            if (_orchestrator != null)
            {
                Console.WriteLine("[WORKFLOW] Emitting OrderPlacedEvent...");

                var orderPlacedEvent = new OrderPlacedEvent(
                    orderNumber,
                    persoana.Nume.Name,
                    persoana.Email.email,
                    persoana.Adress.adress,
                    (decimal)totalComanda,
                    numarProduse,
                    cos.GetProduseCos().Select(p => new OrderItemInfo
                    {
                        ProductCode = p.CodProd.Cod,
                        ProductName = p.Nume,
                        Quantity = (decimal)p.Cantitate.Cantitate,
                        UnitPrice = (decimal)p.Price.pret,
                        LineTotal = (decimal)(p.Price.pret * p.Cantitate.Cantitate)
                    }).ToList()
                );

                await _orchestrator.ProcessOrderAsync(orderPlacedEvent);
            }

            return new ComandaEvent.ComandaPlasataSuccessEvent(
                persoana.Nume.Name,
                totalComanda,
                numarProduse,
                orderNumber,
                null
            );
        }
        catch (Exception ex)
        {
            return new ComandaEvent.ComandaPlasataFailedEvent($"Eroare la procesarea comenzii: {ex.Message}");
        }
    }

    /// <summary>
    /// Synchronous wrapper for compatibility
    /// </summary>
    public static ComandaEvent.IComandaEvent PlaseazaComanda(Persoana persoana, CosDeCumparaturi cos)
    {
        // Validare date intrare
        if (persoana == null || cos == null)
            return new ComandaEvent.ComandaPlasataFailedEvent("Datele de intrare nu sunt valide.");

        // Verificare adresa livrare
        if (persoana.Adress.adress.Length < 5)
            return new ComandaEvent.ComandaPlasataFailedEvent("Adresa de livrare invalida.");

        // Verificare stare cos
        var stareCos = cos.GetStareCos();

        return stareCos switch
        {
            UnvalidatedCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul este invalid, generati un cos nou"),
            EmptyCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul este gol, adaugati produse in cos"),
            ValidatedCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul nu este platit, platiti cosul inainte de a plasa comanda"),
            PayedCos => ProcessareComandaSync(persoana, cos),
            _ => new ComandaEvent.ComandaPlasataFailedEvent("Stare cos necunoscuta.")
        };
    }

    private static ComandaEvent.IComandaEvent ProcessareComandaSync(Persoana persoana, CosDeCumparaturi cos)
    {
        try
        {
            var totalComanda = cos.TotalCos();
            var numarProduse = cos.GetProduseCos().Count;

            return new ComandaEvent.ComandaPlasataSuccessEvent(
                persoana.Nume.Name,
                totalComanda,
                numarProduse,
                Guid.NewGuid()
            );
        }
        catch (Exception ex)
        {
            return new ComandaEvent.ComandaPlasataFailedEvent($"Eroare la procesarea comenzii: {ex.Message}");
        }
    }
}

public sealed class OrderPlacedIntegrationMessage : Lucrarea1PSSC.clase.Infrastructure.Messaging.IMessage
{
    public Guid MessageId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string MessageType => "OrderPlacedIntegration";

    public Guid OrderNumber { get; }
    public string CustomerName { get; }
    public string CustomerEmail { get; }
    public string DeliveryAddress { get; }
    public decimal TotalAmount { get; }
    public int TotalItems { get; }

    public OrderPlacedIntegrationMessage(
        Guid orderNumber,
        string customerName,
        string customerEmail,
        string deliveryAddress,
        decimal totalAmount,
        int totalItems)
    {
        OrderNumber = orderNumber;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        DeliveryAddress = deliveryAddress;
        TotalAmount = totalAmount;
        TotalItems = totalItems;
    }
}
