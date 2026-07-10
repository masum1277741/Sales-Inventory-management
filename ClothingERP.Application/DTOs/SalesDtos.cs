namespace ClothingERP.Application.DTOs;

public class SalesInvoiceListDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public bool IsCredit { get; set; }
    public bool IsHold { get; set; }
}

public class SalesInvoiceDto : SalesInvoiceListDto
{
    public int? CustomerId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public List<SalesInvoiceItemDto> Items { get; set; } = new();
    public List<SalesPaymentDto> Payments { get; set; } = new();
    public object TotalAmountBDT { get; set; }
    public object TotalAmountMVR { get; set; }
}

public class SalesInvoiceItemDto
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class CreateSalesInvoiceDto
{
    public int? CustomerId { get; set; }
    [Range(0, 9999999)]
    public decimal DiscountAmount { get; set; }
    [Range(0, 9999999)]
    public decimal TaxAmount { get; set; }
    public bool IsCredit { get; set; }
    public string? Notes { get; set; }
    [Required, MinLength(1, ErrorMessage = "At least one item is required")]
    public List<CreateSalesInvoiceItemDto> Items { get; set; } = new();
    public List<CreateSalesPaymentDto> Payments { get; set; } = new();
    public decimal ExchangeRateBDT { get; set; } = 110m;
    public decimal ExchangeRateMVR { get; set; } = 15.42m;
    // ── Loyalty ───────────────────────────────────────────────────────────
    public int LoyaltyPointsRedeemed { get; set; } = 0;
}

public class CreateSalesInvoiceItemDto
{
    [Required]
    public int ProductVariantId { get; set; }
    [Required, Range(0.01, 99999)]
    public decimal Quantity { get; set; }
    [Required, Range(0, 9999999)]
    public decimal UnitPrice { get; set; }
    [Range(0, 9999999)]
    public decimal DiscountAmount { get; set; }
    [Range(0, 9999999)]
    public decimal TaxAmount { get; set; }
    public int? ProductBundleId { get; set; }
    public string? BundleName { get; set; }
}

public class SalesPaymentDto
{
    public int Id { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class CreateSalesPaymentDto
{
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    [Required, Range(0.01, 9999999)]
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? GiftCardCode { get; set; }
}