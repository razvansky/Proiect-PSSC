namespace Lucrarea1PSSC.clase.ClaseCos
{
    public static class CartEvents
    {
        public interface ICartEvent 
        {
            DateTime Timestamp { get; }
        }

        // 1. CosCreat - Cart Created
        public record CosCreatEvent : ICartEvent
        {
            internal CosCreatEvent(string numePersoana, DateTime timestamp)
            {
                NumePersoana = numePersoana;
                Timestamp = timestamp;
            }

            public string NumePersoana { get; }
            public DateTime Timestamp { get; }
        }

        // 2. ProdusAdaugatInCos - Product Added to Cart (PUBLISHED)
        public record ProdusAdaugatInCosEvent : ICartEvent
        {
            internal ProdusAdaugatInCosEvent(
                string numeProdus,
                int codProdus,
                double cantitate,
                double pret,
                DateTime timestamp)
            {
                NumeProdus = numeProdus;
                CodProdus = codProdus;
                Cantitate = cantitate;
                Pret = pret;
                Timestamp = timestamp;
            }

            public string NumeProdus { get; }
            public int CodProdus { get; }
            public double Cantitate { get; }
            public double Pret { get; }
            public DateTime Timestamp { get; }
        }

        // 3. ProdusStergeDinCos - Product Removed from Cart (PUBLISHED)
        public record ProdusStergeDinCosEvent : ICartEvent
        {
            internal ProdusStergeDinCosEvent(
                string numeProdus,
                int codProdus,
                double cantitate,
                DateTime timestamp)
            {
                NumeProdus = numeProdus;
                CodProdus = codProdus;
                Cantitate = cantitate;
                Timestamp = timestamp;
            }

            public string NumeProdus { get; }
            public int CodProdus { get; }
            public double Cantitate { get; }
            public DateTime Timestamp { get; }
        }

        // 4. CosGolit - Cart Emptied (PUBLISHED)
        public record CosGolitEvent : ICartEvent
        {
            internal CosGolitEvent(
                List<(string Nume, int Cod, double Cantitate)> produseReturnate,
                DateTime timestamp)
            {
                ProduseReturnate = produseReturnate;
                Timestamp = timestamp;
            }

            public List<(string Nume, int Cod, double Cantitate)> ProduseReturnate { get; }
            public DateTime Timestamp { get; }
        }

        // 5. StareCosSchimbata - Cart State Changed
        public record StareCosSchimbataEvent : ICartEvent
        {
            internal StareCosSchimbataEvent(
                string stareVeche,
                string stareNoua,
                DateTime timestamp)
            {
                StareVeche = stareVeche;
                StareNoua = stareNoua;
                Timestamp = timestamp;
            }

            public string StareVeche { get; }
            public string StareNoua { get; }
            public DateTime Timestamp { get; }
        }

        // 6. CosValidat - Cart Validated
        public record CosValidatEvent : ICartEvent
        {
            internal CosValidatEvent(
                int numarProduse,
                double totalCos,
                DateTime timestamp)
            {
                NumarProduse = numarProduse;
                TotalCos = totalCos;
                Timestamp = timestamp;
            }

            public int NumarProduse { get; }
            public double TotalCos { get; }
            public DateTime Timestamp { get; }
        }

        // 7. CosPlatit - Cart Paid (PUBLISHED - triggers Order Management)
        public record CosPlatitEvent : ICartEvent
        {
            internal CosPlatitEvent(
                string numePersoana,
                double totalPlatit,
                int numarProduse,
                DateTime timestamp)
            {
                NumePersoana = numePersoana;
                TotalPlatit = totalPlatit;
                NumarProduse = numarProduse;
                Timestamp = timestamp;
            }

            public string NumePersoana { get; }
            public double TotalPlatit { get; }
            public int NumarProduse { get; }
            public DateTime Timestamp { get; }
        }
    }
}