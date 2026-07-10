namespace ClothingERP.Domain.Entities;

public class SalesInvoiceItem : BaseEntity
{
    public int SalesInvoiceId { get; set; }
    public int ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? ProductBundleId { get; set; }   // null হলে normal item, না হলে bundle থেকে এসেছে
    public string? BundleName { get; set; }   // receipt/report এ গ্রুপ করে দেখানোর জন্য

    public ProductBundle? ProductBundle { get; set; }
    public virtual SalesInvoice SalesInvoice { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}