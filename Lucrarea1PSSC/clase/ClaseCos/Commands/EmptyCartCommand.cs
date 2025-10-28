namespace Lucrarea1PSSC.clase.ClaseCos.Commands
{
    /// <summary>
    /// Command to empty the shopping cart (remove all products)
    /// </summary>
    public record EmptyCartCommand
    {
        internal EmptyCartCommand() { }

        /// <summary>
        /// Factory method to create the command
        /// </summary>
        public static EmptyCartCommand Create() => new EmptyCartCommand();
    }
}
