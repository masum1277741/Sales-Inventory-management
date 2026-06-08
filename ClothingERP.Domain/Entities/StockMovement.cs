namespace ClothingERP.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int StockId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal PreviousQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Reason { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    public virtual Stock Stock { get; set; } = null!;
}