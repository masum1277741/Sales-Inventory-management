namespace ClothingERP.Domain.Entities;

public class StockTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public string Status { get; set; } = "Pending";  // Pending | InTransit | Received | Cancelled
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedDate { get; set; }
    public int? ReceivedBy { get; set; }
    public string? Notes { get; set; }

    public Branch FromBranch { get; set; } = null!;
    public Branch ToBranch { get; set; } = null!;
    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}