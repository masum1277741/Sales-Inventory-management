namespace ClothingERP.Domain.Entities;

public class SubCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}