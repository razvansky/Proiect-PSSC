using Lucrarea1PSSC.clase.Workflow;
using System;

namespace Lucrarea1PSSC.clase.Workflow.Commands
{
    /// <summary>
    /// Command for picking up an order for preparation
    /// Validates all business rules before the order can be picked up
    /// </summary>
    public sealed class PreluareComandaCommand
    {
        public Guid ComandaId { get; }
        public string OperatorName { get; }
        public DateTime PickupTime { get; }

        private PreluareComandaCommand(Guid comandaId, string operatorName, DateTime pickupTime)
        {
            ComandaId = comandaId;
            OperatorName = operatorName;
            PickupTime = pickupTime;
        }

        /// <summary>
        /// Factory method to create a PreluareComandaCommand with validation
        /// </summary>
        /// <param name="comandaId">The order ID to pick up</param>
        /// <param name="operatorName">The name of the operator picking up the order</param>
        /// <returns>Tuple with success status, command (if successful), and error message (if failed)</returns>
        public static (bool Success, PreluareComandaCommand? Command, string? Error) TryCreate(
            Guid comandaId,
            string operatorName)
        {
            // Validate order ID
            if (comandaId == Guid.Empty)
            {
                return (false, null, "ID-ul comenzii nu poate fi gol");
            }

            // Validate operator name
            if (string.IsNullOrWhiteSpace(operatorName))
            {
                return (false, null, "Numele operatorului nu poate fi gol");
            }

            if (operatorName.Length < 2)
            {
                return (false, null, "Numele operatorului trebuie sa aiba cel putin 2 caractere");
            }

            if (operatorName.Length > 100)
            {
                return (false, null, "Numele operatorului nu poate depasi 100 de caractere");
            }

            return (true, new PreluareComandaCommand(comandaId, operatorName.Trim(), DateTime.UtcNow), null);
        }

        /// <summary>
        /// Factory method to create a PreluareComandaCommand from an existing order
        /// </summary>
        public static (bool Success, PreluareComandaCommand? Command, string? Error) TryCreateFromOrder(
            ComandaAggregate order,
            string operatorName)
        {
            if (order == null)
            {
                return (false, null, "Comanda nu poate fi null");
            }

            // Validate order state - can only pick up placed orders
            if (order.Stare != StaraComanda.Plasata)
            {
                return (false, null, $"Comanda nu poate fi preluata din starea {order.Stare}. Comanda trebuie sa fie in starea 'Plasata'");
            }

            return TryCreate(order.ComandaId, operatorName);
        }
    }
}
