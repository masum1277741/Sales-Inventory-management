namespace ClothingERP.Application.DTOs;

public class StockListDto
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReorderPoint { get; set; }
    public decimal StockValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int BranchId { get; set; }              
    public string BranchName { get; set; } = string.Empty;
}

public class StockDto : StockListDto
{
    public List<StockMovementDto> Movements { get; set; } = new();
}

public class StockMovementDto
{
    public int Id { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime MovementDate { get; set; }
}

public class StockAdjustmentDto
{
    [Required]
    public int ProductVariantId { get; set; }
    [Required, Range(0, 9999999)]
    public decimal NewQuantity { get; set; }
    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    
    public int BranchId { get; set; } = 0;
}