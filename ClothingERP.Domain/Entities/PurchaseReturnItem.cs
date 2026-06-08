namespace ClothingERP.Domain.Entities;

public class PurchaseReturnItem : BaseEntity
{
    public int PurchaseReturnId { get; set; }
    public int ProductVariantId { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? DefectDescription { get; set; }

    public virtual PurchaseReturn PurchaseReturn { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}