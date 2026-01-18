namespace Lucrarea1PSSC.clase.ClaseGestionarePersoane
{
    public static class CustomerEvents
    {
        public interface ICustomerEvent
        {
            DateTime Timestamp { get; }
        }

        // 1. CosAsociatClientului
        public record CosAsociatClientuluiEvent : ICustomerEvent
        {
            internal CosAsociatClientuluiEvent(
                string numeClient,
                int indexCos,
                DateTime timestamp)
            {
                NumeClient = numeClient;
                IndexCos = indexCos;
                Timestamp = timestamp;
            }

            public string NumeClient { get; }
            public int IndexCos { get; }
            public DateTime Timestamp { get; }
        }

        // 2. IstoriCCosActualizat
        public record IstoricCosActualizatEvent : ICustomerEvent
        {
            internal IstoricCosActualizatEvent(
                string numeClient,
                int numarTotalCosuri,
                DateTime timestamp)
            {
                NumeClient = numeClient;
                NumarTotalCosuri = numarTotalCosuri;
                Timestamp = timestamp;
            }

            public string NumeClient { get; }
            public int NumarTotalCosuri { get; }
            public DateTime Timestamp { get; }
        }
    }
}