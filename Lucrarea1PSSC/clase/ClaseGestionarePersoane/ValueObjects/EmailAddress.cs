using System;
using System.Text.RegularExpressions;

namespace Lucrarea1PSSC.clase.ClaseGestionarePersoane.ValueObjects
{
    // Create value object for EmailAddress representing a customer's email
    // Validation rules:
    // - Must match standard email format (user@domain.extension)
    // - Local part (before @) must be 1-64 characters
    // - Domain part must contain at least one dot
    // - Must be lowercase for consistency
    // - Cannot contain spaces or special characters except . _ - +
    // Valid examples: "john.doe@example.com", "customer_123@shop.ro", "support+orders@company.co.uk"
    // Invalid examples: "", "invalid", "no@domain", "@example.com", "user @example.com", "UPPERCASE@DOMAIN.COM"
    // Follow the pattern from copilot-instructions.md
    
    /// <summary>
    /// Represents a customer's email address as a value object
    /// Ensures emails are valid, normalized to lowercase, and follow RFC standards
    /// </summary>
    public record EmailAddress
    {
        private static readonly Regex ValidEmailPattern = new(
            @"^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        
        public string Value { get; }
        
        private EmailAddress(string value)
        {
            var normalized = value.Trim().ToLowerInvariant();
            
            if (IsValid(normalized))
                Value = normalized;
            else
                throw new InvalidEmailAddressException($"Invalid email address: {value}. Must be in format user@domain.ext");
        }
        
        private static bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (value.Length > 254) return false; // RFC 5321
            if (!ValidEmailPattern.IsMatch(value)) return false;
            
            var parts = value.Split('@');
            if (parts.Length != 2) return false;
            if (parts[0].Length == 0 || parts[0].Length > 64) return false; // Local part max 64 chars
            if (parts[1].Length < 3 || !parts[1].Contains('.')) return false; // Domain must have extension
            
            return true;
        }
        
        /// <summary>
        /// Tries to parse a string into an EmailAddress
        /// </summary>
        public static bool TryParse(string input, out EmailAddress? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            var normalized = input.Trim().ToLowerInvariant();
            if (!IsValid(normalized)) return false;
            
            try 
            { 
                result = new(input); 
                return true; 
            }
            catch 
            { 
                return false; 
            }
        }
        
        public override string ToString() => Value;
    }
    
    public class InvalidEmailAddressException : Exception
    {
        public InvalidEmailAddressException(string message) : base(message) { }
    }
}
