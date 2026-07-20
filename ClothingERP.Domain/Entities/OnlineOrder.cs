namespace ClothingERP.Domain.Entities;

public class OnlineOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;   // ORD-20260629-0001
    public int? CustomerId { get; set; }                    // null হলে guest checkout

    // ── Guest Checkout Info (CustomerId null হলে এগুলো ব্যবহার হবে) ─────────
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }

    // ── Shipping ──────────────────────────────────────────────────────────
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string? ShippingNotes { get; set; }

    public decimal SubtotalUSD { get; set; }
    public decimal ShippingFeeUSD { get; set; }
    public decimal TotalUSD { get; set; }
    public string Currency { get; set; } = "USD";  // visitor যেই currency দিয়ে দেখেছিল (display purpose)

    public string PaymentMethod { get; set; } = "COD";  // COD | bKash | Nagad | Card
    public string PaymentStatus { get; set; } = "Pending"; // Pending | Paid | Failed
    public string? DigitalPaymentId { get; set; }           // Feature #20 এর gateway payment id, থাকলে

    public string Status { get; set; } = "Placed";
    // Placed | Confirmed | Processing | Shipped | Delivered | Cancelled | Returned
    public int? FulfillmentBranchId { get; set; }        // Feature #21 — কোন branch থেকে stock কমলো
    public int? SalesInvoiceId { get; set; }            // Delivered হলে ERP invoice এর সাথে link
    public string? CancellationReason { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<OnlineOrderItem> Items { get; set; } = new List<OnlineOrderItem>();
}