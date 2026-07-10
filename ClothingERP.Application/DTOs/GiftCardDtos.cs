namespace ClothingERP.Application.DTOs;

public class GiftCardListDto
{
    public int Id { get; set; }
    public string CardCode { get; set; } = string.Empty;
    public decimal InitialValue { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? CustomerName { get; set; }
    public string? RecipientName { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsStoreCredit { get; set; }
}

public class GiftCardDto : GiftCardListDto
{
    public string? Notes { get; set; }
    public List<GiftCardTransactionDto> Transactions { get; set; } = new();
}

public class GiftCardTransactionDto
{
    public int Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public int? SalesInvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }
}

// ── Issue নতুন (বিক্রিত) Gift Card ──────────────────────────────────────
public class IssueGiftCardDto
{
    [Required, Range(1, 100000)] public decimal Amount { get; set; }
    public int? CustomerId { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }

    [Required] public string PaymentMethod { get; set; } = "Cash"; // কীভাবে gift card এর টাকা নেওয়া হলো
}

// ── Store Credit Issue (return/refund থেকে) ──────────────────────────────
public class IssueStoreCreditDto
{
    [Required] public int CustomerId { get; set; }
    [Required, Range(0.01, 100000)] public decimal Amount { get; set; }
    public int? SourceReturnId { get; set; }
    public string? Notes { get; set; }
}

// ── Lookup (POS এর জন্য) ───────────────────────────────────────────────
public class GiftCardLookupDto
{
    public bool Found { get; set; }
    public string? Message { get; set; }
    public string? CardCode { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? Status { get; set; }
    public bool IsUsable { get; set; }
}