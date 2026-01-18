using System;

namespace Lucrarea1PSSC.clase.ClaseCos.Commands
{
    /// <summary>
    /// Command to add a product to the shopping cart
    /// </summary>
    public record AddProductToCartCommand
    {
        internal AddProductToCartCommand(string numeProdus)
        {
            if (string.IsNullOrWhiteSpace(numeProdus))
                throw new ArgumentException("Numele produsului nu poate fi gol");

            NumeProdus = numeProdus;
        }

        public string NumeProdus { get; }

        /// <summary>
        /// Factory method for safe command creation with validation
        /// </summary>
        public static (bool Success, AddProductToCartCommand? Command, string? Error) TryCreate(string numeProdus)
        {
            if (string.IsNullOrWhiteSpace(numeProdus))
                return (false, null, "Numele produsului nu poate fi gol");

            try
            {
                var command = new AddProductToCartCommand(numeProdus);
                return (true, command, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
