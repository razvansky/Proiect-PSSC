using System;

namespace Lucrarea1PSSC.clase.ClaseProduse.Commands
{
    /// <summary>
    /// Command to decrease product stock (when added to cart)
    /// </summary>
    public record DecreaseStockCommand
    {
        internal DecreaseStockCommand(int codProdus, double cantitate)
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
        public static (bool Success, DecreaseStockCommand? Command, string? Error) TryCreate(
            int codProdus, 
            double cantitate)
        {
            if (codProdus <= 0)
                return (false, null, "Codul produsului trebuie s? fie pozitiv");

            if (cantitate <= 0)
                return (false, null, "Cantitatea trebuie s? fie pozitiv?");

            try
            {
                var command = new DecreaseStockCommand(codProdus, cantitate);
                return (true, command, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
