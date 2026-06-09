namespace ClothingERP.Web.Models;

public class BarcodeLabelViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal RetailPrice { get; set; }
    public string? SKU { get; set; }
    public int PrintQty { get; set; } = 1;
}