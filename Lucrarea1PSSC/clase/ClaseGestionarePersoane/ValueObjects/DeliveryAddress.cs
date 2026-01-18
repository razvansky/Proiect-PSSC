using System;
using System.Text.RegularExpressions;

namespace Lucrarea1PSSC.clase.ClaseGestionarePersoane.ValueObjects
{
    // Create value object for DeliveryAddress representing a customer's delivery address
    // Validation rules:
    // - Street must be between 5 and 100 characters
    // - City must be between 2 and 50 characters
    // - Postal code must match Romanian format (6 digits) or international format
    // - Country must be specified (default: Romania)
    // - Cannot contain only numbers or special characters
    // - Must have at least street and city
    // Valid examples: "Str. Mihai Eminescu 15, Cluj-Napoca, 400347, Romania", "Bulevardul Unirii 1, Bucure?ti, 030823", "123 Main St, New York, 10001, USA"
    // Invalid examples: "", "Street", "123", "A, B", "Very long address that exceeds the maximum allowed length for a street address and should be rejected by validation rules"
    // Follow the pattern from copilot-instructions.md
    
    /// <summary>
    /// Represents a delivery address as a value object
    /// Ensures addresses are complete, valid, and properly formatted for delivery
    /// </summary>
    public record DeliveryAddress
    {
        private static readonly Regex RomanianPostalCodePattern = new(@"^\d{6}$", RegexOptions.Compiled);
        private static readonly Regex InternationalPostalCodePattern = new(@"^[A-Z0-9\s\-]{3,10}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        private const int MinStreetLength = 5;
        private const int MaxStreetLength = 100;
        private const int MinCityLength = 2;
        private const int MaxCityLength = 50;
        
        public string Street { get; }
        public string City { get; }
        public string PostalCode { get; }
        public string Country { get; }
        
        private DeliveryAddress(string street, string city, string postalCode, string country)
        {
            var normalizedStreet = street.Trim();
            var normalizedCity = city.Trim();
            var normalizedPostalCode = postalCode.Trim();
            var normalizedCountry = country.Trim();
            
            if (IsValid(normalizedStreet, normalizedCity, normalizedPostalCode, normalizedCountry))
            {
                Street = normalizedStreet;
                City = normalizedCity;
                PostalCode = normalizedPostalCode;
                Country = normalizedCountry;
            }
            else
                throw new InvalidDeliveryAddressException(
                    $"Invalid delivery address. " +
                    $"Street must be {MinStreetLength}-{MaxStreetLength} chars, " +
                    $"City must be {MinCityLength}-{MaxCityLength} chars, " +
                    $"Postal code must be valid, Country required.");
        }
        
        private static bool IsValid(string street, string city, string postalCode, string country)
        {
            // Street validation
            if (string.IsNullOrWhiteSpace(street)) return false;
            if (street.Length < MinStreetLength || street.Length > MaxStreetLength) return false;
            if (street.All(char.IsDigit)) return false; // Cannot be only numbers
            
            // City validation
            if (string.IsNullOrWhiteSpace(city)) return false;
            if (city.Length < MinCityLength || city.Length > MaxCityLength) return false;
            if (city.All(char.IsDigit)) return false; // Cannot be only numbers
            
            // Postal code validation
            if (string.IsNullOrWhiteSpace(postalCode)) return false;
            var isRomanianPostalCode = RomanianPostalCodePattern.IsMatch(postalCode);
            var isInternationalPostalCode = InternationalPostalCodePattern.IsMatch(postalCode);
            if (!isRomanianPostalCode && !isInternationalPostalCode) return false;
            
            // Country validation
            if (string.IsNullOrWhiteSpace(country)) return false;
            if (country.Length < 2 || country.Length > 50) return false;
            
            return true;
        }
        
        /// <summary>
        /// Creates a DeliveryAddress
        /// </summary>
        public static DeliveryAddress Create(string street, string city, string postalCode, string country = "Romania")
            => new(street, city, postalCode, country);
        
        /// <summary>
        /// Tries to parse a full address string (format: "Street, City, PostalCode, Country")
        /// </summary>
        public static bool TryParse(string input, out DeliveryAddress? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            
            var parts = input.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 2) return false;
            
            var street = parts[0];
            var city = parts[1];
            var postalCode = parts.Length > 2 ? parts[2] : "";
            var country = parts.Length > 3 ? parts[3] : "Romania";
            
            // If postal code not provided, try to extract from city
            if (string.IsNullOrWhiteSpace(postalCode))
            {
                // Try to find 6-digit postal code in city string
                var match = Regex.Match(city, @"\d{6}");
                if (match.Success)
                {
                    postalCode = match.Value;
                    city = city.Replace(postalCode, "").Trim();
                }
                else
                {
                    postalCode = "000000"; // Default for incomplete addresses
                }
            }
            
            try 
            { 
                result = new(street, city, postalCode, country); 
                return true; 
            }
            catch 
            { 
                return false; 
            }
        }
        
        /// <summary>
        /// Full address as formatted string
        /// </summary>
        public string FullAddress => $"{Street}, {City}, {PostalCode}, {Country}";
        
        /// <summary>
        /// Short address (without country)
        /// </summary>
        public string ShortAddress => $"{Street}, {City}, {PostalCode}";
        
        public override string ToString() => FullAddress;
    }
    
    public class InvalidDeliveryAddressException : Exception
    {
        public InvalidDeliveryAddressException(string message) : base(message) { }
    }
}
