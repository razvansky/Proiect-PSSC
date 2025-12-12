using System;
using System.Collections.Generic;
using Lucrarea1PSSC.clase.ClaseGestionarePersoane.ValueObjects;

namespace Lucrarea1PSSC.clase.ClaseGestionarePersoane.Entities
{
    // Create entity states for Customer following the pattern from copilot-instructions.md
    // 
    // State flow:
    // UnvalidatedCustomer ? ValidatedCustomer ? ActiveCustomer ? PremiumCustomer
    //                    ? InvalidCustomer                      ? InactiveCustomer
    //
    // States needed:
    // 1. UnvalidatedCustomer: Raw input with string properties: name, email, address, phone
    // 2. ValidatedCustomer: After validation with value objects: Name, Email, Address
    // 3. ActiveCustomer: After first purchase with PurchaseHistory, LoyaltyPoints
    // 4. PremiumCustomer: VIP customer with Discount, PriorityShipping
    // 5. InactiveCustomer: No purchases in last period with LastActivityDate
    // 6. InvalidCustomer: When validation fails, contains Reasons
    //
    // Each state implements ICustomer interface
    // Use internal constructors and IReadOnlyCollection for lists
    
    /// <summary>
    /// Customer entity with lifecycle states
    /// Represents customer progression from registration to premium status
    /// </summary>
    public static class CustomerEntity
    {
        /// <summary>
        /// Base interface for all customer states
        /// </summary>
        public interface ICustomer { }
        
        /// <summary>
        /// Initial unvalidated state - raw registration data
        /// Contains string-based properties before validation
        /// </summary>
        public record UnvalidatedCustomer : ICustomer
        {
            internal UnvalidatedCustomer(
                string name,
                string email,
                string address,
                string? phone)
            {
                Name = name;
                Email = email;
                Address = address;
                Phone = phone;
            }
            
            public string Name { get; }
            public string Email { get; }
            public string Address { get; }
            public string? Phone { get; }
        }
        
        /// <summary>
        /// Validated state - registration data validated
        /// Customer account created but no purchases yet
        /// </summary>
        public record ValidatedCustomer : ICustomer
        {
            internal ValidatedCustomer(
                string customerId,
                string name,
                EmailAddress email,
                DeliveryAddress primaryAddress,
                DateTime registeredAt)
            {
                CustomerId = customerId;
                Name = name;
                Email = email;
                PrimaryAddress = primaryAddress;
                RegisteredAt = registeredAt;
            }
            
            public string CustomerId { get; }
            public string Name { get; }
            public EmailAddress Email { get; }
            public DeliveryAddress PrimaryAddress { get; }
            public DateTime RegisteredAt { get; }
        }
        
        /// <summary>
        /// Active state - customer has made purchases
        /// Contains purchase history and loyalty information
        /// </summary>
        public record ActiveCustomer : ICustomer
        {
            internal ActiveCustomer(
                string customerId,
                string name,
                EmailAddress email,
                DeliveryAddress primaryAddress,
                IReadOnlyCollection<DeliveryAddress> savedAddresses,
                DateTime registeredAt,
                DateTime lastPurchaseDate,
                int totalOrders,
                double totalSpent,
                int loyaltyPoints)
            {
                CustomerId = customerId;
                Name = name;
                Email = email;
                PrimaryAddress = primaryAddress;
                SavedAddresses = savedAddresses;
                RegisteredAt = registeredAt;
                LastPurchaseDate = lastPurchaseDate;
                TotalOrders = totalOrders;
                TotalSpent = totalSpent;
                LoyaltyPoints = loyaltyPoints;
            }
            
            public string CustomerId { get; }
            public string Name { get; }
            public EmailAddress Email { get; }
            public DeliveryAddress PrimaryAddress { get; }
            public IReadOnlyCollection<DeliveryAddress> SavedAddresses { get; }
            public DateTime RegisteredAt { get; }
            public DateTime LastPurchaseDate { get; }
            public int TotalOrders { get; }
            public double TotalSpent { get; }
            public int LoyaltyPoints { get; }
        }
        
        /// <summary>
        /// Premium state - VIP customer with special benefits
        /// Earned through high purchase volume or loyalty
        /// </summary>
        public record PremiumCustomer : ICustomer
        {
            internal PremiumCustomer(
                string customerId,
                string name,
                EmailAddress email,
                DeliveryAddress primaryAddress,
                IReadOnlyCollection<DeliveryAddress> savedAddresses,
                DateTime registeredAt,
                DateTime lastPurchaseDate,
                int totalOrders,
                double totalSpent,
                int loyaltyPoints,
                DateTime premiumSince,
                double discountPercentage,
                bool priorityShipping,
                bool freeReturns,
                string tierName)
            {
                CustomerId = customerId;
                Name = name;
                Email = email;
                PrimaryAddress = primaryAddress;
                SavedAddresses = savedAddresses;
                RegisteredAt = registeredAt;
                LastPurchaseDate = lastPurchaseDate;
                TotalOrders = totalOrders;
                TotalSpent = totalSpent;
                LoyaltyPoints = loyaltyPoints;
                PremiumSince = premiumSince;
                DiscountPercentage = discountPercentage;
                PriorityShipping = priorityShipping;
                FreeReturns = freeReturns;
                TierName = tierName;
            }
            
            public string CustomerId { get; }
            public string Name { get; }
            public EmailAddress Email { get; }
            public DeliveryAddress PrimaryAddress { get; }
            public IReadOnlyCollection<DeliveryAddress> SavedAddresses { get; }
            public DateTime RegisteredAt { get; }
            public DateTime LastPurchaseDate { get; }
            public int TotalOrders { get; }
            public double TotalSpent { get; }
            public int LoyaltyPoints { get; }
            public DateTime PremiumSince { get; }
            public double DiscountPercentage { get; }
            public bool PriorityShipping { get; }
            public bool FreeReturns { get; }
            public string TierName { get; } // e.g., "Gold", "Platinum", "Diamond"
        }
        
        /// <summary>
        /// Inactive state - customer hasn't purchased recently
        /// Can be reactivated with special offers
        /// </summary>
        public record InactiveCustomer : ICustomer
        {
            internal InactiveCustomer(
                string customerId,
                string name,
                EmailAddress email,
                DeliveryAddress primaryAddress,
                DateTime registeredAt,
                DateTime lastPurchaseDate,
                DateTime inactiveSince,
                int totalOrders,
                double totalSpent,
                string inactivityReason)
            {
                CustomerId = customerId;
                Name = name;
                Email = email;
                PrimaryAddress = primaryAddress;
                RegisteredAt = registeredAt;
                LastPurchaseDate = lastPurchaseDate;
                InactiveSince = inactiveSince;
                TotalOrders = totalOrders;
                TotalSpent = totalSpent;
                InactivityReason = inactivityReason;
            }
            
            public string CustomerId { get; }
            public string Name { get; }
            public EmailAddress Email { get; }
            public DeliveryAddress PrimaryAddress { get; }
            public DateTime RegisteredAt { get; }
            public DateTime LastPurchaseDate { get; }
            public DateTime InactiveSince { get; }
            public int TotalOrders { get; }
            public double TotalSpent { get; }
            public string InactivityReason { get; }
        }
        
        /// <summary>
        /// Invalid state - validation or processing failed
        /// Contains reasons for failure
        /// </summary>
        public record InvalidCustomer : ICustomer
        {
            internal InvalidCustomer(IEnumerable<string> reasons)
            {
                Reasons = reasons;
            }
            
            public IEnumerable<string> Reasons { get; }
        }
    }
    
    /// <summary>
    /// Customer tier levels for premium customers
    /// </summary>
    public enum CustomerTier
    {
        Standard,    // New customers
        Silver,      // 5+ orders or 1000 RON spent
        Gold,        // 20+ orders or 5000 RON spent
        Platinum,    // 50+ orders or 15000 RON spent
        Diamond      // 100+ orders or 50000 RON spent
    }
}
