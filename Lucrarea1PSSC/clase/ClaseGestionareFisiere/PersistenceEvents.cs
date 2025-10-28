namespace Lucrarea1PSSC.clase.ClaseGestionareFisiere
{
    public static class PersistenceEvents
    {
        public interface IPersistenceEvent
        {
            DateTime Timestamp { get; }
        }

        // DateSalvateInFisier
        public record DateSalvateInFisierEvent : IPersistenceEvent
        {
            internal DateSalvateInFisierEvent(
                string tipFisier,
                string caleFisier,
                int numarInregistrari,
                DateTime timestamp)
            {
                TipFisier = tipFisier;
                CaleFisier = caleFisier;
                NumarInregistrari = numarInregistrari;
                Timestamp = timestamp;
            }

            public string TipFisier { get; }
            public string CaleFisier { get; }
            public int NumarInregistrari { get; }
            public DateTime Timestamp { get; }
        }

        // EroareCitireFisier
        public record EroareCitireFisierEvent : IPersistenceEvent
        {
            internal EroareCitireFisierEvent(
                string caleFisier,
                string mesajEroare,
                DateTime timestamp)
            {
                CaleFisier = caleFisier;
                MesajEroare = mesajEroare;
                Timestamp = timestamp;
            }

            public string CaleFisier { get; }
            public string MesajEroare { get; }
            public DateTime Timestamp { get; }
        }
    }
}