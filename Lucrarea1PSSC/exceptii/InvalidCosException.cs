using System;

namespace Lucrarea1PSSC.exceptii
{
    /// <summary>
    /// Exception thrown when shopping cart operations fail due to invalid state
    /// </summary>
    public class InvalidCosException : Exception
    {
        /// <summary>
        /// Gets the error code associated with this exception
        /// </summary>
        public string? ErrorCode { get; }

        public InvalidCosException() : base("Operatiune invalida asupra cosului de cumparaturi")
        {
        }

        public InvalidCosException(string? message) : base(message)
        {
        }

        public InvalidCosException(string? message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public InvalidCosException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public InvalidCosException(string? message, string errorCode, Exception? innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
