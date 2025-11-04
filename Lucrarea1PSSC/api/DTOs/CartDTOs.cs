using System;
using System.Collections.Generic;

namespace Lucrarea1PSSC.api.DTOs
{
    /// <summary>
    /// DTO for viewing shopping cart
    /// </summary>
    public record ViewCartResponse
    {
        public string CustomerName { get; init; } = string.Empty;
        public string CartStatus { get; init; } = string.Empty;
        public List<CartItemDto> Items { get; init; } = new();
        public decimal TotalAmount { get; init; }
        public int TotalItems { get; init; }
        public DateTime? LastModified { get; init; }
    }

    /// <summary>
    /// DTO for cart item
    /// </summary>
    public record CartItemDto
    {
        public int ProductCode { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal LineTotal { get; init; }
        public string QuantityType { get; init; } = "Unit";
        public double KilogramQuantity { get; init; }
    }

    /// <summary>
    /// Request to add product to cart
    /// </summary>
    public record AddProductToCartRequest
    {
        public string CustomerName { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public decimal Quantity { get; init; } = 1;
    }

    /// <summary>
    /// Response after adding product to cart
    /// </summary>
    public record AddProductToCartResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public CartItemDto? AddedItem { get; init; }
        public decimal NewCartTotal { get; init; }
        public int TotalItems { get; init; }
    }

    /// <summary>
    /// Request to mark cart as paid
    /// </summary>
    public record MarkCartAsPaidRequest
    {
        public string CustomerName { get; init; } = string.Empty;
        public string PaymentMethod { get; init; } = "Card";
        public string? TransactionId { get; init; }
    }

    /// <summary>
    /// Response after marking cart as paid
    /// </summary>
    public record MarkCartAsPaidResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public decimal TotalPaid { get; init; }
        public int ItemsPaid { get; init; }
        public DateTime PaymentDate { get; init; }
        public string? TransactionId { get; init; }
    }

    /// <summary>
    /// Standard error response
    /// </summary>
    public record ErrorResponse
    {
        public string Error { get; init; } = string.Empty;
        public string? Details { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
