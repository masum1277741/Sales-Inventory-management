namespace ClothingERP.Application.DTOs;

public class GlobalSearchResultDto
{
    public string Category { get; set; } = string.Empty;  // "Products", "Customers", "Invoices", ইত্যাদি
    public string Icon { get; set; } = string.Empty;  // bootstrap icon class
    public List<SearchResultItemDto> Items { get; set; } = new();
}

public class SearchResultItemDto
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Badge { get; set; }     // যেমন: status, due amount
    public string? BadgeColor { get; set; }   // "success", "danger", "warning"
}