namespace ClothingERP.Application.DTOs;

// ── Sales Return ──────────────────────────────────────────────────────────
public class SalesReturnListDto
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string ReturnType { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal RefundAmount { get; set; }
}

public class SalesReturnDto : SalesReturnListDto
{
    public int SalesInvoiceId { get; set; }
    public int? CustomerId { get; set; }
    public string? Reason { get; set; }
    public string RefundMethod { get; set; } = string.Empty;
    public List<SalesReturnItemDto> Items { get; set; } = new();
}

public class SalesReturnItemDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public decimal ReturnQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? DefectDescription { get; set; }
}

public class CreateSalesReturnDto
{
    [Required]
    public int SalesInvoiceId { get; set; }
    [Required]
    public ReturnType ReturnType { get; set; }
    public string? Reason { get; set; }
    [Range(0, 9999999)]
    public decimal RefundAmount { get; set; }
    [Required]
    public RefundMethod RefundMethod { get; set; }
    [Required, MinLength(1)]
    public List<CreateSalesReturnItemDto> Items { get; set; } = new();
}

public class CreateSalesReturnItemDto
{
    [Required]
    public int ProductVariantId { get; set; }
    [Required, Range(0.01, 99999)]
    public decimal ReturnQuantity { get; set; }
    [Required, Range(0, 9999999)]
    public decimal UnitPrice { get; set; }
    public string? DefectDescription { get; set; }
}

// ── Purchase Return ───────────────────────────────────────────────────────
public class PurchaseReturnListDto
{
    public int Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string PONumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public decimal TotalAmount { get; set; }
}

public class PurchaseReturnDto : PurchaseReturnListDto
{
    public int PurchaseOrderId { get; set; }
    public int SupplierId { get; set; }
    public string? Reason { get; set; }
    public List<PurchaseReturnItemDto> Items { get; set; } = new();
}

public class PurchaseReturnItemDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public decimal ReturnQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? DefectDescription { get; set; }
}

public class CreatePurchaseReturnDto
{
    [Required]
    public int PurchaseOrderId { get; set; }
    [Required]
    public int SupplierId { get; set; }
    public string? Reason { get; set; }
    [Required, MinLength(1)]
    public List<CreatePurchaseReturnItemDto> Items { get; set; } = new();
}

public class CreatePurchaseReturnItemDto
{
    [Required]
    public int ProductVariantId { get; set; }
    [Required, Range(0.01, 99999)]
    public decimal ReturnQuantity { get; set; }
    [Required, Range(0, 9999999)]
    public decimal UnitCost { get; set; }
    public string? DefectDescription { get; set; }
}