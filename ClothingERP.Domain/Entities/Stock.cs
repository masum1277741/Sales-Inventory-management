namespace ClothingERP.Domain.Entities;

public class Stock : BaseEntity
{
    public int ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ReservedQuantity { get; set; }

    [NotMapped]
    public decimal AvailableQuantity => Quantity - ReservedQuantity;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}