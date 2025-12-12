using System;

namespace Lucrarea1PSSC.clase.ClaseCos.Commands
{
    /// <summary>
    /// Command to remove a product from the shopping cart
    /// </summary>
    public record RemoveProductFromCartCommand
    {
        internal RemoveProductFromCartCommand(string numeProdus)
        {
            if (string.IsNullOrWhiteSpace(numeProdus))
                throw new ArgumentException("Numele produsului nu poate fi gol");

            NumeProdus = numeProdus;
        }

        public string NumeProdus { get; }

        /// <summary>
        /// Factory method for safe command creation with validation
        /// </summary>
        public static (bool Success, RemoveProductFromCartCommand? Command, string? Error) TryCreate(string numeProdus)
        {
            if (string.IsNullOrWhiteSpace(numeProdus))
                return (false, null, "Numele produsului nu poate fi gol");

            try
            {
                var command = new RemoveProductFromCartCommand(numeProdus);
                return (true, command, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
