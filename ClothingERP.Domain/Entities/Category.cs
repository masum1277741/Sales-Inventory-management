namespace ClothingERP.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
}