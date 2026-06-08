namespace ClothingERP.Domain.Entities;

public class Size : BaseEntity
{
    public string Name { get; set; } = string.Empty; // S, M, L, XL, XXL
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}