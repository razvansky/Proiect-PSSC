using System;

// IMPORTANT: No namespace - must match PlasareComandaWorkflow.cs which is also in global namespace
public static partial class ComandaEvent
{
    // AdresaValidata - Address Validated Event
    public record AdresaValidataEvent : IComandaEvent
    {
        internal AdresaValidataEvent(string adresa, DateTime timestamp)
        {
            Adresa = adresa;
            Timestamp = timestamp;
        }

        public string Adresa { get; }
        public DateTime Timestamp { get; }
    }

    // AdresaInvalida - Address Invalid Event
    public record AdresaInvalidaEvent : IComandaEvent
    {
        internal AdresaInvalidaEvent(string adresa, string motiv, DateTime timestamp)
        {
            Adresa = adresa;
            Motiv = motiv;
            Timestamp = timestamp;
        }

        public string Adresa { get; }
        public string Motiv { get; }
        public DateTime Timestamp { get; }
    }

    // ComandaPregatitaPentruPlasare - Order Ready for Placement
    public record ComandaPregatitaPentruPlasareEvent : IComandaEvent
    {
        internal ComandaPregatitaPentruPlasareEvent(string numePersoana, double total, DateTime timestamp)
        {
            NumePersoana = numePersoana;
            Total = total;
            Timestamp = timestamp;
        }

        public string NumePersoana { get; }
        public double Total { get; }
        public DateTime Timestamp { get; }
    }
}