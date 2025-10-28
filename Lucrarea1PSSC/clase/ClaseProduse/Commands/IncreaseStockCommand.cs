using System;

namespace Lucrarea1PSSC.clase.ClaseProduse.Commands
{
    /// <summary>
    /// Command to increase product stock (when removed from cart)
    /// </summary>
    public record IncreaseStockCommand
    {
        internal IncreaseStockCommand(int codProdus, double cantitate)
        {
            if (codProdus <= 0)
                throw new ArgumentException("Codul produsului trebuie s? fie pozitiv");
            
            if (cantitate <= 0)
                throw new ArgumentException("Cantitatea trebuie s? fie pozitiv?");

            CodProdus = codProdus;
            Cantitate = cantitate;
        }

        public int CodProdus { get; }
        public double Cantitate { get; }

        /// <summary>
        /// Factory method for safe command creation with validation
        /// </summary>
        public static (bool Success, IncreaseStockCommand? Command, string? Error) TryCreate(
            int codProdus, 
            double cantitate)
        {
            if (codProdus <= 0)
                return (false, null, "Codul produsului trebuie s? fie pozitiv");

            if (cantitate <= 0)
                return (false, null, "Cantitatea trebuie s? fie pozitiv?");

            try
            {
                var command = new IncreaseStockCommand(codProdus, cantitate);
                return (true, command, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
