namespace ClothingERP.Domain.Entities;

public class StockTransferItem : BaseEntity
{
    public int StockTransferId { get; set; }
    public int ProductVariantId { get; set; }
    public int RequestedQty { get; set; }
    public int? ReceivedQty { get; set; }  

    public StockTransfer StockTransfer { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}