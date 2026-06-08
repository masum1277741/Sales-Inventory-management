namespace ClothingERP.Domain.Entities;

public class Color : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string HexCode { get; set; } = "#000000";
    public bool IsActive { get; set; } = true;
}