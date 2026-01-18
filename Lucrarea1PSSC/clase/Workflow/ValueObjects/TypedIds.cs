using System;
using Lucrarea1PSSC.clase.Infrastructure;

namespace Lucrarea1PSSC.clase.Workflow.ValueObjects
{
    /// <summary>
    /// Money value object - replaces primitive double for currency amounts
    /// Ensures type safety and prevents mixing currencies.
    /// </summary>
    public sealed record Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        private Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new InvalidMoneyException("Amount cannot be negative");

            if (string.IsNullOrWhiteSpace(currency))
                throw new InvalidMoneyException("Currency code is required");

            if (currency.Length != 3)
                throw new InvalidMoneyException("Currency code must be 3 characters (ISO 4217)");

            Amount = amount;
            Currency = currency.ToUpperInvariant();
        }

        public static Money FromDecimal(decimal amount, string currency = "RON") => new(amount, currency);
        public static Money FromDouble(double amount, string currency = "RON") => new((decimal)amount, currency);
        public static Money Zero(string currency = "RON") => new(0, currency);

        public static Result<Money> TryParse(string input, string defaultCurrency = "RON")
        {
            if (string.IsNullOrWhiteSpace(input))
                return DomainError.ValidationFailed("Money value cannot be empty");

            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return DomainError.ValidationFailed("Invalid money format");

            if (!decimal.TryParse(parts[0], out var amount))
                return DomainError.ValidationFailed($"Invalid amount: {parts[0]}");

            var currency = parts.Length > 1 ? parts[1] : defaultCurrency;

            try
            {
                return new Money(amount, currency);
            }
            catch (InvalidMoneyException ex)
            {
                return DomainError.ValidationFailed(ex.Message);
            }
        }

        public Money Add(Money other)
        {
            EnsureSameCurrency(other);
            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            EnsureSameCurrency(other);
            var result = Amount - other.Amount;
            if (result < 0)
                throw new InvalidOperationException("Result would be negative");

            return new Money(result, Currency);
        }

        public Money Multiply(decimal factor)
        {
            if (factor < 0)
                throw new InvalidOperationException("Factor cannot be negative");

            return new Money(Amount * factor, Currency);
        }

        public Money CalculatePercentage(decimal percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new InvalidOperationException("Percentage must be between 0 and 100");

            return new Money(Amount * percentage / 100, Currency);
        }

        private void EnsureSameCurrency(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Currency mismatch: {Currency} vs {other.Currency}");
        }

        public override string ToString() => $"{Amount:F2} {Currency}";

        public double ToDouble() => (double)Amount;

        public static Money operator +(Money left, Money right) => left.Add(right);
        public static Money operator -(Money left, Money right) => left.Subtract(right);
        public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
        public static Money operator *(decimal factor, Money money) => money.Multiply(factor);
    }

    public class InvalidMoneyException : Exception
    {
        public InvalidMoneyException(string message) : base(message) { }
    }

    // NOTE: Money has been moved to ValueObjects/Money.cs

    /// <summary>
    /// Strongly-typed Order identifier
    /// Prevents ID mix-ups between different entity types
    /// Follows value object pattern from copilot_instructions_full.md
    /// </summary>
    public sealed record OrderId
    {
        public Guid Value { get; }

        private OrderId(Guid value)
        {
            if (value == Guid.Empty)
                throw new InvalidOrderIdException("Order ID cannot be empty");

            Value = value;
        }

        /// <summary>
        /// Create a new unique order ID
        /// </summary>
        public static OrderId Create() => new(Guid.NewGuid());

        /// <summary>
        /// Create from existing GUID (for reconstruction from persistence)
        /// </summary>
        public static OrderId FromGuid(Guid value)
        {
            if (value == Guid.Empty)
                throw new InvalidOrderIdException("Order ID cannot be empty GUID");

            return new OrderId(value);
        }

        /// <summary>
        /// Try to parse string representation
        /// </summary>
        public static Result<OrderId> TryParse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return DomainError.ValidationFailed("Order ID cannot be empty");

            if (!Guid.TryParse(input, out var guid))
                return DomainError.ValidationFailed($"Invalid Order ID format: {input}");

            if (guid == Guid.Empty)
                return DomainError.ValidationFailed("Order ID cannot be empty GUID");

            return new OrderId(guid);
        }

        /// <summary>
        /// Try to parse with out parameter (traditional .NET style)
        /// </summary>
        public static bool TryParse(string input, out OrderId? orderId)
        {
            orderId = null;

            if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                return false;

            orderId = new OrderId(guid);
            return true;
        }

        public override string ToString() => Value.ToString();

        /// <summary>
        /// For display in UI with prefix
        /// </summary>
        public string ToDisplayString() => $"ORD-{Value:N}";
    }

    /// <summary>
    /// Strongly-typed Invoice identifier
    /// </summary>
    public sealed record InvoiceId
    {
        public Guid Value { get; }

        private InvoiceId(Guid value)
        {
            if (value == Guid.Empty)
                throw new InvalidInvoiceIdException("Invoice ID cannot be empty");

            Value = value;
        }

        public static InvoiceId Create() => new(Guid.NewGuid());
        public static InvoiceId FromGuid(Guid value) => value != Guid.Empty
            ? new InvoiceId(value)
            : throw new InvalidInvoiceIdException("Invoice ID cannot be empty");

        public static Result<InvoiceId> TryParse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return DomainError.ValidationFailed("Invoice ID cannot be empty");

            if (!Guid.TryParse(input, out var guid))
                return DomainError.ValidationFailed($"Invalid Invoice ID format: {input}");

            if (guid == Guid.Empty)
                return DomainError.ValidationFailed("Invoice ID cannot be empty GUID");

            return new InvoiceId(guid);
        }

        public override string ToString() => Value.ToString();

        /// <summary>
        /// Generate invoice number in format: INV-YYYYMMDD-NNNNNN
        /// </summary>
        public string ToInvoiceNumber()
        {
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var uniqueSuffix = Value.ToString("N")[..6].ToUpperInvariant();
            return $"INV-{datePrefix}-{uniqueSuffix}";
        }
    }

    /// <summary>
    /// Strongly-typed Shipment identifier
    /// </summary>
    public sealed record ShipmentId
    {
        public Guid Value { get; }

        private ShipmentId(Guid value)
        {
            if (value == Guid.Empty)
                throw new InvalidShipmentIdException("Shipment ID cannot be empty");

            Value = value;
        }

        public static ShipmentId Create() => new(Guid.NewGuid());
        public static ShipmentId FromGuid(Guid value) => value != Guid.Empty
            ? new ShipmentId(value)
            : throw new InvalidShipmentIdException("Shipment ID cannot be empty");

        public static Result<ShipmentId> TryParse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return DomainError.ValidationFailed("Shipment ID cannot be empty");

            if (!Guid.TryParse(input, out var guid))
                return DomainError.ValidationFailed($"Invalid Shipment ID format: {input}");

            if (guid == Guid.Empty)
                return DomainError.ValidationFailed("Shipment ID cannot be empty GUID");

            return new ShipmentId(guid);
        }

        public override string ToString() => Value.ToString();

        /// <summary>
        /// Generate tracking number in format: SHP-YYYYMMDD-NNNNNN
        /// </summary>
        public string ToTrackingNumber()
        {
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var uniqueSuffix = Value.ToString("N")[..6].ToUpperInvariant();
            return $"SHP-{datePrefix}-{uniqueSuffix}";
        }
    }

    public class InvalidOrderIdException : Exception
    {
        public InvalidOrderIdException(string message) : base(message) { }
    }

    public class InvalidInvoiceIdException : Exception
    {
        public InvalidInvoiceIdException(string message) : base(message) { }
    }

    public class InvalidShipmentIdException : Exception
    {
        public InvalidShipmentIdException(string message) : base(message) { }
    }
}
