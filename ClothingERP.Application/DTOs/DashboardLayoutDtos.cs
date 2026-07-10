namespace ClothingERP.Application.DTOs;

public class WidgetConfigDto
{
    public string WidgetKey { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public string Size { get; set; } = "Medium"; // Small | Medium | Large | Full
}

public class WidgetDefinitionDto
{
    public string WidgetKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;  // Stats | Charts | Lists
    public bool AllowResize { get; set; } = true;
}

public class DashboardLayoutDto
{
    public List<WidgetConfigDto> Widgets { get; set; } = new();
}

public class SaveDashboardLayoutDto
{
    [Required] public List<WidgetConfigDto> Widgets { get; set; } = new();
}