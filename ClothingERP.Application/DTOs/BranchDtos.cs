namespace ClothingERP.Application.DTOs;

public class BranchDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; }
    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; }
    public int StaffCount { get; set; }
    public decimal TodaySales { get; set; }
}

public class CreateBranchDto
{
    [Required, MaxLength(20)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Country { get; set; } = "Bangladesh";
    public bool IsActive { get; set; } = true;
}

public class UserBranchAssignmentDto
{
    [Required] public int UserId { get; set; }
    [Required, MinLength(1)] public List<int> BranchIds { get; set; } = new();
    public int? DefaultBranchId { get; set; }
}

public class MyBranchAccessDto
{
    public List<BranchDto> AccessibleBranches { get; set; } = new();
    public int DefaultBranchId { get; set; }
    public bool CanAccessAllBranches { get; set; }  
}

// ── Stock Transfer ───────────────────────────────────────────────────────
public class StockTransferListDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string FromBranchName { get; set; } = string.Empty;
    public string ToBranchName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public DateTime TransferDate { get; set; }
}

public class StockTransferDto : StockTransferListDto
{
    public string? Notes { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
}

public class StockTransferItemDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public int RequestedQty { get; set; }
    public int? ReceivedQty { get; set; }
    public int AvailableAtSource { get; set; }
}

public class CreateStockTransferDto
{
    [Required] public int ToBranchId { get; set; }    
    [Required, MinLength(1)] public List<CreateTransferItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class CreateTransferItemDto
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
}

public class ReceiveStockTransferDto
{
    [Required] public int StockTransferId { get; set; }
    [Required] public List<ReceiveTransferItemDto> Items { get; set; } = new();
}

public class ReceiveTransferItemDto
{
    public int ProductVariantId { get; set; }
    public int ReceivedQty { get; set; }
}

// ── Branch-aware Stock View ───────────────────────────────────────────────
public class BranchStockComparisonDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public List<BranchStockDto> StockByBranch { get; set; } = new();
    public int TotalAcrossBranches { get; set; }
}

public class BranchStockDto
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}