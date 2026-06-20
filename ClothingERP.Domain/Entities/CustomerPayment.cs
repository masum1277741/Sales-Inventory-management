namespace ClothingERP.Domain.Entities;

public class CustomerPayment : BaseEntity
{
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public Customer Customer { get; set; } = null!;
}