namespace ClothingERP.Application.DTOs;

public class AppModuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string? Icon { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}