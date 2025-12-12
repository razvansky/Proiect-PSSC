using Lucrarea1PSSC.clase.ClaseGestionarePersoane;
using Lucrarea1PSSC.clase.ClaseCos;
using System;

namespace Lucrarea1PSSC.clase.Workflow.Commands
{
    /// <summary>
    /// Command to place an order from a paid shopping cart
    /// </summary>
    public record PlaceOrderCommand
    {
        internal PlaceOrderCommand(
            Persoana persoana,
            CosDeCumparaturi cos)
        {
            Persoana = persoana ?? throw new ArgumentNullException(nameof(persoana));
            Cos = cos ?? throw new ArgumentNullException(nameof(cos));
        }

        public Persoana Persoana { get; }
        public CosDeCumparaturi Cos { get; }

        /// <summary>
        /// Factory method for safe command creation with validation
        /// </summary>
        public static (bool Success, PlaceOrderCommand? Command, string? Error) TryCreate(
            Persoana persoana,
            CosDeCumparaturi cos)
        {
            if (persoana == null)
                return (false, null, "Persoana nu poate fi null");

            if (cos == null)
                return (false, null, "Co?ul nu poate fi null");

            try
            {
                var command = new PlaceOrderCommand(persoana, cos);
                return (true, command, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
