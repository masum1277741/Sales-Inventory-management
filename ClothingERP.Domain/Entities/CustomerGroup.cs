namespace ClothingERP.Domain.Entities;

public class CustomerGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Retail, Wholesale, VIP
    public decimal DiscountPercentage { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}