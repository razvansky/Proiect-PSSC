using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static partial class ComandaEvent
{
    public interface IComandaEvent { }

    public record ComandaPlasataSuccessEvent : IComandaEvent
    {
        internal ComandaPlasataSuccessEvent(string numePers, double total, int numarProduse, Guid comandaId)
        {
            NumePersoana = numePers;
            TotalComanda = total;
            NumarProduse = numarProduse;
            ComandaId = comandaId;
        }

        public string NumePersoana { get; }
        public double TotalComanda { get; }
        public int NumarProduse { get; }
        public Guid ComandaId { get; }
        public string Message => $"Comanda a fost plasata cu succes pentru {NumePersoana}. Total: {TotalComanda} lei, Produse: {NumarProduse}";
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

    public PlasareComandaWorkflow(OrderWorkflowDatabaseService? dbService = null)
    {
        _dbService = dbService;
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
                return new ComandaEvent.ComandaPlasataSuccessEvent(
                    persoana.Nume.Name,
                    totalComanda,
                    numarProduse,
                    order!.OrderNumber
                );
            }
            else
            {
                // Fallback if no database service
                return new ComandaEvent.ComandaPlasataSuccessEvent(
                    persoana.Nume.Name,
                    totalComanda,
                    numarProduse,
                    orderNumber
                );
            }
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
