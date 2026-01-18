using System;
using System.Text.RegularExpressions;

namespace Lucrarea1PSSC.clase.Workflow.ValueObjects
{
    // Create value object for OrderNumber representing a unique order identifier
    // Validation rules:
    // - Must be a valid GUID format
    // - Cannot be empty GUID (00000000-0000-0000-0000-000000000000)
    // - Must be immutable once created
    // Valid examples: "3fa85f64-5717-4562-b3fc-2c963f66afa6", "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    // Invalid examples: "", "invalid-guid", "00000000-0000-0000-0000-000000000000"
    // Follow the pattern from copilot-instructions.md
    
    /// <summary>
    /// Represents a unique order number as a value object
    /// Ensures order identifiers are valid and non-empty GUIDs
    /// </summary>
    public record OrderNumber
    {
        public Guid Value { get; }
        
        private OrderNumber(Guid value)
        {
            if (IsValid(value))
                Value = value;
            else
                throw new InvalidOrderNumberException($"Invalid order number: {value}. Order number cannot be empty GUID.");
        }
        
        private static bool IsValid(Guid value) => value != Guid.Empty;
        
        /// <summary>
        /// Tries to parse a string into an OrderNumber
        /// </summary>
        public static bool TryParse(string input, out OrderNumber? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            if (!Guid.TryParse(input, out var guid)) return false;
            if (!IsValid(guid)) return false;
            
            try 
            { 
                result = new(guid); 
                return true; 
            }
            catch 
            { 
                return false; 
            }
        }
        
        /// <summary>
        /// Creates a new OrderNumber with a generated GUID
        /// </summary>
        public static OrderNumber Create() => new(Guid.NewGuid());
        
        /// <summary>
        /// Creates an OrderNumber from an existing GUID
        /// </summary>
        public static OrderNumber FromGuid(Guid guid) => new(guid);
        
        public override string ToString() => Value.ToString();
    }
    
    public class InvalidOrderNumberException : Exception
    {
        public InvalidOrderNumberException(string message) : base(message) { }
    }
}
