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

    // ================================
    // PRELUARE COMANDA (Order Pickup) Events
    // ================================

    /// <summary>
    /// Event emitted when an order is successfully picked up for preparation
    /// </summary>
    public record ComandaPreluataSuccessEvent : IComandaEvent
    {
        public ComandaPreluataSuccessEvent(
            Guid comandaId,
            string numeClient,
            string operatorName,
            double total,
            int numarProduse,
            DateTime pickupTime)
        {
            ComandaId = comandaId;
            NumeClient = numeClient;
            OperatorName = operatorName;
            Total = total;
            NumarProduse = numarProduse;
            PickupTime = pickupTime;
        }

        public Guid ComandaId { get; }
        public string NumeClient { get; }
        public string OperatorName { get; }
        public double Total { get; }
        public int NumarProduse { get; }
        public DateTime PickupTime { get; }

        public string Message => $"Comanda {ComandaId} a fost preluata cu succes de {OperatorName}. " +
                                  $"Client: {NumeClient}, Total: {Total:F2} lei, Produse: {NumarProduse}";
    }

    /// <summary>
    /// Event emitted when order pickup fails
    /// </summary>
    public record ComandaPreluataFailedEvent : IComandaEvent
    {
        public ComandaPreluataFailedEvent(Guid comandaId, string reason, DateTime timestamp)
        {
            ComandaId = comandaId;
            Reason = reason;
            Timestamp = timestamp;
        }

        public Guid ComandaId { get; }
        public string Reason { get; }
        public DateTime Timestamp { get; }

        public string Message => $"Preluarea comenzii {ComandaId} a esuat: {Reason}";
    }

    /// <summary>
    /// Event emitted when order preparation starts
    /// </summary>
    public record ComandaInPregatireEvent : IComandaEvent
    {
        public ComandaInPregatireEvent(
            Guid comandaId,
            string operatorName,
            DateTime startTime,
            TimeSpan estimatedDuration)
        {
            ComandaId = comandaId;
            OperatorName = operatorName;
            StartTime = startTime;
            EstimatedDuration = estimatedDuration;
        }

        public Guid ComandaId { get; }
        public string OperatorName { get; }
        public DateTime StartTime { get; }
        public TimeSpan EstimatedDuration { get; }

        public string Message => $"Comanda {ComandaId} este in pregatire de {OperatorName}. " +
                                  $"Durata estimata: {EstimatedDuration.TotalMinutes} minute";
    }

    /// <summary>
    /// Event emitted when order preparation is complete
    /// </summary>
    public record ComandaPregatitaEvent : IComandaEvent
    {
        public ComandaPregatitaEvent(
            Guid comandaId,
            string operatorName,
            DateTime completionTime,
            bool isReadyForShipment)
        {
            ComandaId = comandaId;
            OperatorName = operatorName;
            CompletionTime = completionTime;
            IsReadyForShipment = isReadyForShipment;
        }

        public Guid ComandaId { get; }
        public string OperatorName { get; }
        public DateTime CompletionTime { get; }
        public bool IsReadyForShipment { get; }

        public string Message => IsReadyForShipment
            ? $"Comanda {ComandaId} a fost pregatita si este gata pentru expediere"
            : $"Comanda {ComandaId} a fost pregatita dar necesita verificari suplimentare";
    }

    /// <summary>
    /// Event emitted when order validation fails during pickup
    /// </summary>
    public record ValidareComandaFailedEvent : IComandaEvent
    {
        public ValidareComandaFailedEvent(
            Guid comandaId,
            string[] validationErrors,
            DateTime timestamp)
        {
            ComandaId = comandaId;
            ValidationErrors = validationErrors;
            Timestamp = timestamp;
        }

        public Guid ComandaId { get; }
        public string[] ValidationErrors { get; }
        public DateTime Timestamp { get; }

        public string Message => $"Validarea comenzii {ComandaId} a esuat cu {ValidationErrors.Length} erori: " +
                                  string.Join("; ", ValidationErrors);
    }
}