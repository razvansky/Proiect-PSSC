using System;

namespace Lucrarea1PSSC.clase.ClaseCos.Commands
{
    /// <summary>
    /// Command to create a new shopping cart for a customer
    /// </summary>
    public record CreateCartCommand
    {
        internal CreateCartCommand(string numePersoana)
        {
            if (string.IsNullOrWhiteSpace(numePersoana))
                throw new ArgumentException("Numele persoanei nu poate fi gol");
            
            NumePersoana = numePersoana;
        }

        public string NumePersoana { get; }

        /// <summary>
        /// Factory method for safe command creation with validation
        /// </summary>
        public static (bool Success, CreateCartCommand? Command, string? Error) TryCreate(string numePersoana)
        {
            if (string.IsNullOrWhiteSpace(numePersoana))
                return (false, null, "Numele persoanei nu poate fi gol");

            try
            {
                var command = new CreateCartCommand(numePersoana);
                return (true, command, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
