using System;
using System.Text.RegularExpressions;

namespace Lucrarea1PSSC.clase.ClaseProduse.ValueObjects
{
    // Create value object for ProductName representing a product's display name
    // Validation rules:
    // - Must be between 2 and 100 characters
    // - Cannot be only whitespace
    // - Cannot contain special characters except space, hyphen, apostrophe, ampersand
    // - Must start with a letter or number
    // - Cannot have consecutive spaces
    // Valid examples: "Laptop Dell XPS 15", "iPhone 14 Pro", "Samsung TV 55\"", "Men's T-Shirt", "Coffee & Tea Set"
    // Invalid examples: "", " ", "A", "Product!!!Name", "  Double  Spaces", "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
    // Follow the pattern from copilot-instructions.md
    
    /// <summary>
    /// Represents a product's name as a value object
    /// Ensures product names are valid, readable, and follow naming standards
    /// </summary>
    public record ProductName
    {
        private static readonly Regex ValidNamePattern = new(
            @"^[a-zA-Z0-9][a-zA-Z0-9\s\-'&""\.]*$",
            RegexOptions.Compiled
        );
        
        private const int MinLength = 2;
        private const int MaxLength = 100;
        
        public string Value { get; }
        
        private ProductName(string value)
        {
            var trimmed = value.Trim();
            var normalized = NormalizeSpaces(trimmed);
            
            if (IsValid(normalized))
                Value = normalized;
            else
                throw new InvalidProductNameException(
                    $"Invalid product name: '{value}'. " +
                    $"Must be {MinLength}-{MaxLength} characters, start with letter/number, " +
                    "and contain only letters, numbers, spaces, hyphens, apostrophes, ampersands, dots, and quotes.");
        }
        
        private static string NormalizeSpaces(string value)
        {
            // Replace multiple consecutive spaces with single space
            return Regex.Replace(value, @"\s+", " ");
        }
        
        private static bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            if (value.Length < MinLength || value.Length > MaxLength) return false;
            if (!ValidNamePattern.IsMatch(value)) return false;
            if (value.Contains("  ")) return false; // No double spaces
            
            return true;
        }
        
        /// <summary>
        /// Tries to parse a string into a ProductName
        /// </summary>
        public static bool TryParse(string input, out ProductName? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            
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
    
    public class InvalidProductNameException : Exception
    {
        public InvalidProductNameException(string message) : base(message) { }
    }
}
