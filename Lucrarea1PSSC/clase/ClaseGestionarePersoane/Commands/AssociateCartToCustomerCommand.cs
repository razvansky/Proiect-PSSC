using Lucrarea1PSSC.clase.ClaseCos;
using System;

namespace Lucrarea1PSSC.clase.ClaseGestionarePersoane.Commands
{
    /// <summary>
    /// Command to associate a shopping cart with a customer
    /// </summary>
    public record AssociateCartToCustomerCommand
    {
        internal AssociateCartToCustomerCommand(
            string numeClient,
            CosDeCumparaturi cos)
        {
            if (string.IsNullOrWhiteSpace(numeClient))
                throw new ArgumentException("Numele clientului nu poate fi gol");

            NumeClient = numeClient;
            Cos = cos ?? throw new ArgumentNullException(nameof(cos));
        }

        public string NumeClient { get; }
        public CosDeCumparaturi Cos { get; }

        /// <summary>
        /// Factory method for safe command creation with validation
        /// </summary>
        public static (bool Success, AssociateCartToCustomerCommand? Command, string? Error) TryCreate(
            string numeClient,
            CosDeCumparaturi cos)
        {
            if (string.IsNullOrWhiteSpace(numeClient))
                return (false, null, "Numele clientului nu poate fi gol");

            if (cos == null)
                return (false, null, "Co?ul nu poate fi null");

            try
            {
                var command = new AssociateCartToCustomerCommand(numeClient, cos);
                return (true, command, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
