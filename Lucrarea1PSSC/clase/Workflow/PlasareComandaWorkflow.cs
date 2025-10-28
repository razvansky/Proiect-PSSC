using Lucrarea1PSSC.clase.ClaseCos;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using System;

public static partial class ComandaEvent  // Changed to partial
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

public static class PlasareComandaWorkflow
{
    public static ComandaEvent.IComandaEvent PlaseazaComanda(Persoana persoana, CosDeCumparaturi cos)
    {
        // Validare date intrare
        if (persoana == null || cos == null)
            return new ComandaEvent.ComandaPlasataFailedEvent("Datele de intrare nu sunt valide.");

        // Verificare adresa livrare
        if (persoana.Adress.adress.Length < 5)
            return new ComandaEvent.ComandaPlasataFailedEvent("Adresa de livrare invalida.");

        // Verificare stare cos folosind pattern matching
        var stareCos = cos.GetStareCos();

        return stareCos switch
        {
            UnvalidatedCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul este invalid, generati un cos nou"),
            EmptyCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul este gol, adaugati produse in cos"),
            ValidatedCos => new ComandaEvent.ComandaPlasataFailedEvent("Cosul nu este platit, platiti cosul inainte de a plasa comanda"),
            PayedCos => ProcessareComanda(persoana, cos),
            _ => new ComandaEvent.ComandaPlasataFailedEvent("Stare cos necunoscuta.")
        };
    }

    private static ComandaEvent.IComandaEvent ProcessareComanda(Persoana persoana, CosDeCumparaturi cos)
    {
        try
        {
            var totalComanda = cos.TotalCos();
            var numarProduse = cos.GetProduseCos().Count;

            // Aici poti adauga logica suplimentara:
            // - Generare numar comanda
            // - Salvare in baza de date
            // - Trimitere email confirmare
            // - etc.

            return new ComandaEvent.ComandaPlasataSuccessEvent(
                persoana.Nume.Name,
                totalComanda,
                numarProduse,
                Guid.NewGuid() // Generate order ID
            );
        }
        catch (Exception ex)
        {
            return new ComandaEvent.ComandaPlasataFailedEvent($"Eroare la procesarea comenzii: {ex.Message}");
        }
    }
}
