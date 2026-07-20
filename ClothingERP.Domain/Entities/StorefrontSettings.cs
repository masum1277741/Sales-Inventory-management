namespace ClothingERP.Domain.Entities;

public class StorefrontSettings : BaseEntity
{
    public bool IsStoreEnabled { get; set; } = true;
    public string StoreName { get; set; } = "CLOZEY";
    public string StoreTagline { get; set; } = "Attractive Beauty — Online";
    public decimal FlatShippingFeeUSD { get; set; } = 3m;
    public decimal FreeShippingThresholdUSD { get; set; } = 50m;
    public bool CodEnabled { get; set; } = true;
    public bool BkashEnabled { get; set; } = false;
    public bool NagadEnabled { get; set; } = false;
    public int FulfillmentBranchId { get; set; }   // Feature #21 — কোন branch থেকে stock কমবে
    public string? BannerImagePath { get; set; }
    public string? AnnouncementText { get; set; }   // "Eid Sale: 20% Off!" ধরনের ব্যানার টেক্সট
}