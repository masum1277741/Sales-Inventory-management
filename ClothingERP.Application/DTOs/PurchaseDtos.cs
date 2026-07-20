namespace ClothingERP.Application.DTOs;

// ── Purchase Order ────────────────────────────────────────────────────────
public class PurchaseOrderListDto
{
    public int Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
}

public class PurchaseOrderDto : PurchaseOrderListDto
{
    public int SupplierId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalCost { get; set; }
}

public class CreatePurchaseOrderDto
{
    [Required]
    public int SupplierId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    [Range(0, 9999999)]
    public decimal DiscountAmount { get; set; }
    [Range(0, 9999999)]
    public decimal TaxAmount { get; set; }
    [Range(0, 9999999)]
    public decimal ShippingCost { get; set; }
    public string? Notes { get; set; }
    [Required, MinLength(1, ErrorMessage = "At least one item is required")]
    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new();
    public int BranchId { get; set; }
}

public class CreatePurchaseOrderItemDto
{
    [Required]
    public int ProductVariantId { get; set; }
    [Required, Range(0.01, 99999)]
    public decimal Quantity { get; set; }
    [Required, Range(0, 9999999)]
    public decimal UnitCost { get; set; }
    [Range(0, 9999999)]
    public decimal DiscountAmount { get; set; }
}

// ── GRN ──────────────────────────────────────────────────────────────────
public class GRNListDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public string PONumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public decimal TotalValue { get; set; }
}

public class GRNDto : GRNListDto
{
    public int PurchaseOrderId { get; set; }
    public int SupplierId { get; set; }
    public string? DeliveryChallan { get; set; }
    public string? Notes { get; set; }
    public List<GRNItemDto> Items { get; set; } = new();
}

public class GRNItemDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
}

public class CreateGRNDto
{
    [Required]
    public int PurchaseOrderId { get; set; }
    public string? DeliveryChallan { get; set; }
    public string? Notes { get; set; }
    [Required, MinLength(1)]
    public List<CreateGRNItemDto> Items { get; set; } = new();
}

public class CreateGRNItemDto
{
    [Required]
    public int PurchaseOrderItemId { get; set; }
    [Required]
    public int ProductVariantId { get; set; }
    [Required, Range(0.01, 99999)]
    public decimal ReceivedQuantity { get; set; }
    [Required, Range(0, 9999999)]
    public decimal UnitCost { get; set; }
}