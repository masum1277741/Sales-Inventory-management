namespace ClothingERP.Domain.Entities;

public class SalesReturnItem : BaseEntity
{
    public int SalesReturnId { get; set; }
    public int ProductVariantId { get; set; }
    public decimal ReturnQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? DefectDescription { get; set; }

    public virtual SalesReturn SalesReturn { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}