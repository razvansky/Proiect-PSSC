using Lucrarea1PSSC.clase.ClaseCos;

namespace Lucrarea1PSSC.clase.ClaseProduse
{
    public static class InventoryEvents
    {
        public interface IInventoryEvent
        {
            DateTime Timestamp { get; }
        }

        // 1. StocProdusScazut - Stock Decreased
        public record StocProdusScazutEvent : IInventoryEvent
        {
            internal StocProdusScazutEvent(
                int codProdus,
                string numeProdus,
                double cantitateScazuta,
                double stocRamas,
                DateTime timestamp)
            {
                CodProdus = codProdus;
                NumeProdus = numeProdus;
                CantitateScazuta = cantitateScazuta;
                StocRamas = stocRamas;
                Timestamp = timestamp;
            }

            public int CodProdus { get; }
            public string NumeProdus { get; }
            public double CantitateScazuta { get; }
            public double StocRamas { get; }
            public DateTime Timestamp { get; }
        }

        // 2. StocProdusMarit - Stock Increased
        public record StocProdusMaritEvent : IInventoryEvent
        {
            internal StocProdusMaritEvent(
                int codProdus,
                string numeProdus,
                double cantitateAdaugata,
                double stocNou,
                DateTime timestamp)
            {
                CodProdus = codProdus;
                NumeProdus = numeProdus;
                CantitateAdaugata = cantitateAdaugata;
                StocNou = stocNou;
                Timestamp = timestamp;
            }

            public int CodProdus { get; }
            public string NumeProdus { get; }
            public double CantitateAdaugata { get; }
            public double StocNou { get; }
            public DateTime Timestamp { get; }
        }

        // 3. ProdusEpuizat - Product Out of Stock (PUBLISHED)
        public record ProdusEpuizatEvent : IInventoryEvent
        {
            internal ProdusEpuizatEvent(
                int codProdus,
                string numeProdus,
                DateTime timestamp)
            {
                CodProdus = codProdus;
                NumeProdus = numeProdus;
                Timestamp = timestamp;
            }

            public int CodProdus { get; }
            public string NumeProdus { get; }
            public DateTime Timestamp { get; }
        }

        // 4. ProdusDisponibil - Product Available Again
        public record ProdusDisponibilEvent : IInventoryEvent
        {
            internal ProdusDisponibilEvent(
                int codProdus,
                string numeProdus,
                double stocDisponibil,
                DateTime timestamp)
            {
                CodProdus = codProdus;
                NumeProdus = numeProdus;
                StocDisponibil = stocDisponibil;
                Timestamp = timestamp;
            }

            public int CodProdus { get; }
            public string NumeProdus { get; }
            public double StocDisponibil { get; }
            public DateTime Timestamp { get; }
        }
    }
}