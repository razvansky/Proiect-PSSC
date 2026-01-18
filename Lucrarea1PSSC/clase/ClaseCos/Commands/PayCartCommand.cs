namespace Lucrarea1PSSC.clase.ClaseCos.Commands
{
    /// <summary>
    /// Command to pay for the shopping cart
    /// </summary>
    public record PayCartCommand
    {
        internal PayCartCommand() { }

        /// <summary>
        /// Factory method to create the command
        /// </summary>
        public static PayCartCommand Create() => new PayCartCommand();
    }
}
