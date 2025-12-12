using System;
using System.Globalization;

namespace Lucrarea1PSSC.clase.ClaseProduse.ValueObjects
{
    // Create value object for Money representing monetary amounts with currency
    // Validation rules:
    // - Amount must be non-negative (>= 0)
    // - Amount must have maximum 2 decimal places for standard currencies
    // - Currency code must be valid ISO 4217 (3 letters)
    // - Amount cannot exceed maximum safe value for double (prevent overflow)
    // - Default currency is RON (Romanian Leu) if not specified
    // Valid examples: "100 RON", "2500.50 EUR", "0.99 USD", "1000000 RON"
    // Invalid examples: "-50 RON", "100.999 EUR", "100 ABC", "100", ""
    // Follow the pattern from copilot-instructions.md
    
    /// <summary>
    /// Represents a monetary amount with currency as a value object
    /// Ensures amounts are valid, non-negative, and associated with proper currency
    /// </summary>
    public record Money
    {
        private const int MaxDecimalPlaces = 2;
        private const double MaxAmount = 1_000_000_000.0; // 1 billion max
        
        public double Amount { get; }
        public string Currency { get; }
        
        private Money(double amount, string currency)
        {
            if (IsValid(amount, currency))
            {
                Amount = Math.Round(amount, MaxDecimalPlaces);
                Currency = currency.ToUpperInvariant();
            }
            else
                throw new InvalidMoneyException(
                    $"Invalid money: {amount} {currency}. " +
                    $"Amount must be non-negative, max 2 decimals, and currency must be 3-letter code.");
        }
        
        private static bool IsValid(double amount, string currency)
        {
            if (amount < 0) return false;
            if (amount > MaxAmount) return false;
            if (double.IsNaN(amount) || double.IsInfinity(amount)) return false;
            
            // Check decimal places
            var rounded = Math.Round(amount, MaxDecimalPlaces);
            if (Math.Abs(amount - rounded) > 0.001) return false;
            
            if (string.IsNullOrWhiteSpace(currency)) return false;
            if (currency.Length != 3) return false;
            if (!IsValidCurrencyCode(currency)) return false;
            
            return true;
        }
        
        private static bool IsValidCurrencyCode(string code)
        {
            var validCurrencies = new[] { "RON", "EUR", "USD", "GBP", "CHF", "PLN", "HUF", "BGN", "CZK", "SEK", "NOK", "DKK" };
            return Array.Exists(validCurrencies, c => c.Equals(code, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Creates Money from amount and currency
        /// </summary>
        public static Money FromAmount(double amount, string currency = "RON") 
            => new(amount, currency);
        
        /// <summary>
        /// Tries to parse a string into Money (format: "100.50 RON")
        /// </summary>
        public static bool TryParse(string input, out Money? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return false;
            
            if (!double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var amount))
                return false;
            
            var currency = parts[1];
            
            try 
            { 
                result = new(amount, currency); 
                return true; 
            }
            catch 
            { 
                return false; 
            }
        }
        
        /// <summary>
        /// Add two Money values (must be same currency)
        /// </summary>
        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot add different currencies: {Currency} and {other.Currency}");
            
            return new Money(Amount + other.Amount, Currency);
        }
        
        /// <summary>
        /// Subtract two Money values (must be same currency)
        /// </summary>
        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot subtract different currencies: {Currency} and {other.Currency}");
            
            var result = Amount - other.Amount;
            if (result < 0)
                throw new InvalidOperationException("Subtraction would result in negative amount");
            
            return new Money(result, Currency);
        }
        
        /// <summary>
        /// Multiply Money by a factor
        /// </summary>
        public Money Multiply(double factor)
        {
            if (factor < 0)
                throw new InvalidOperationException("Cannot multiply by negative factor");
            
            return new Money(Amount * factor, Currency);
        }
        
        public override string ToString() => $"{Amount:F2} {Currency}";
    }
    
    public class InvalidMoneyException : Exception
    {
        public InvalidMoneyException(string message) : base(message) { }
    }
}
