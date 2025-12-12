using System;
using System.Collections.Generic;
using Lucrarea1PSSC.clase.ClaseProduse;
using Lucrarea1PSSC.clase.ClaseProduse.ValueObjects;

namespace Lucrarea1PSSC.clase.ClaseCos.Entities
{
    // Create entity states for ShoppingCart following the pattern from copilot-instructions.md
    // 
    // State flow:
    // UnvalidatedShoppingCart ? ValidatedShoppingCart ? PayedShoppingCart ? CompletedShoppingCart
    //                        ? InvalidShoppingCart
    //
    // States needed:
    // 1. UnvalidatedShoppingCart: Raw input with string properties: customerName, productNames[]
    // 2. ValidatedShoppingCart: After validation with value objects: Customer, Products[], Total
    // 3. PayedShoppingCart: After payment with PaymentMethod, PaymentDate
    // 4. CompletedShoppingCart: After order placement with OrderId, CompletionDate
    // 5. InvalidShoppingCart: When validation/processing fails, contains Reasons
    //
    // Each state implements IShoppingCart interface
    // Use internal constructors and IReadOnlyCollection for lists
    
    /// <summary>
    /// Shopping cart entity with lifecycle states
    /// Represents the complete flow from creation to order placement
    /// </summary>
    public static class ShoppingCartEntity
    {
        /// <summary>
        /// Base interface for all shopping cart states
        /// </summary>
        public interface IShoppingCart { }
        
        /// <summary>
        /// Initial unvalidated state - raw input data from user
        /// Contains string-based properties before validation
        /// </summary>
        public record UnvalidatedShoppingCart : IShoppingCart
        {
            internal UnvalidatedShoppingCart(
                string customerName,
                IEnumerable<string> productNames)
            {
                CustomerName = customerName;
                ProductNames = productNames;
            }
            
            public string CustomerName { get; }
            public IEnumerable<string> ProductNames { get; }
        }
        
        /// <summary>
        /// Validated state - all data validated with value objects
        /// Cart has products and calculated total
        /// </summary>
        public record ValidatedShoppingCart : IShoppingCart
        {
            internal ValidatedShoppingCart(
                string customerId,
                IReadOnlyCollection<CartProduct> products,
                Money total,
                DateTime createdAt)
            {
                CustomerId = customerId;
                Products = products;
                Total = total;
                CreatedAt = createdAt;
            }
            
            public string CustomerId { get; }
            public IReadOnlyCollection<CartProduct> Products { get; }
            public Money Total { get; }
            public DateTime CreatedAt { get; }
        }
        
        /// <summary>
        /// Paid state - payment processed successfully
        /// Contains payment information and is ready for order creation
        /// </summary>
        public record PayedShoppingCart : IShoppingCart
        {
            internal PayedShoppingCart(
                string customerId,
                IReadOnlyCollection<CartProduct> products,
                Money total,
                DateTime createdAt,
                PaymentMethod paymentMethod,
                string transactionId,
                DateTime paymentDate)
            {
                CustomerId = customerId;
                Products = products;
                Total = total;
                CreatedAt = createdAt;
                PaymentMethod = paymentMethod;
                TransactionId = transactionId;
                PaymentDate = paymentDate;
            }
            
            public string CustomerId { get; }
            public IReadOnlyCollection<CartProduct> Products { get; }
            public Money Total { get; }
            public DateTime CreatedAt { get; }
            public PaymentMethod PaymentMethod { get; }
            public string TransactionId { get; }
            public DateTime PaymentDate { get; }
        }
        
        /// <summary>
        /// Completed state - order has been placed from this cart
        /// Final state in the cart lifecycle
        /// </summary>
        public record CompletedShoppingCart : IShoppingCart
        {
            internal CompletedShoppingCart(
                string customerId,
                IReadOnlyCollection<CartProduct> products,
                Money total,
                DateTime createdAt,
                PaymentMethod paymentMethod,
                string transactionId,
                DateTime paymentDate,
                Guid orderId,
                DateTime completionDate)
            {
                CustomerId = customerId;
                Products = products;
                Total = total;
                CreatedAt = createdAt;
                PaymentMethod = paymentMethod;
                TransactionId = transactionId;
                PaymentDate = paymentDate;
                OrderId = orderId;
                CompletionDate = completionDate;
            }
            
            public string CustomerId { get; }
            public IReadOnlyCollection<CartProduct> Products { get; }
            public Money Total { get; }
            public DateTime CreatedAt { get; }
            public PaymentMethod PaymentMethod { get; }
            public string TransactionId { get; }
            public DateTime PaymentDate { get; }
            public Guid OrderId { get; }
            public DateTime CompletionDate { get; }
        }
        
        /// <summary>
        /// Invalid state - validation or processing failed
        /// Contains reasons for failure
        /// </summary>
        public record InvalidShoppingCart : IShoppingCart
        {
            internal InvalidShoppingCart(IEnumerable<string> reasons)
            {
                Reasons = reasons;
            }
            
            public IEnumerable<string> Reasons { get; }
        }
    }
    
    /// <summary>
    /// Product in cart with quantity
    /// </summary>
    public record CartProduct
    {
        internal CartProduct(
            int productCode,
            ProductName productName,
            int quantity,
            Money unitPrice,
            Money totalPrice)
        {
            ProductCode = productCode;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TotalPrice = totalPrice;
        }
        
        public int ProductCode { get; }
        public ProductName ProductName { get; }
        public int Quantity { get; }
        public Money UnitPrice { get; }
        public Money TotalPrice { get; }
    }
    
    /// <summary>
    /// Payment method enumeration
    /// </summary>
    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        DebitCard,
        BankTransfer,
        OnlinePayment
    }
}
