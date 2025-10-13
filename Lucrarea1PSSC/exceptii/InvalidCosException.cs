using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucrarea1PSSC.exceptii
{
    internal class InvalidCosException : Exception
    {
        public InvalidCosException() { }
        public InvalidCosException(string? message) : base(message)
        {
        }

        public InvalidCosException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
