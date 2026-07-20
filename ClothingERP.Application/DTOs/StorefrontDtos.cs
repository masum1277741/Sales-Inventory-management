namespace ClothingERP.Application.DTOs;

// ── Catalog Browsing ─────────────────────────────────────────────────────
public class StorefrontProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public decimal MinPriceUSD { get; set; }
    public decimal MaxPriceUSD { get; set; }
    public bool InStock { get; set; }
    public List<string> AvailableSizes { get; set; } = new();
    public List<string> AvailableColors { get; set; } = new();
}

public class StorefrontProductDetailDto : StorefrontProductDto
{
    public List<StorefrontVariantDto> Variants { get; set; } = new();
}

public class StorefrontVariantDto
{
    public int VariantId { get; set; }
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public decimal PriceUSD { get; set; }
    public int StockQty { get; set; }
    public bool InStock { get; set; }
}

public class ProductFilterDto
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortBy { get; set; } = "Newest";  // Newest | PriceLowHigh | PriceHighLow | Name
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// ── Cart / Checkout (cart টা client-side localStorage এ থাকে, checkout এ পাঠানো হয়) ──
public class CartItemDto
{
    public int VariantId { get; set; }
    public int Quantity { get; set; }
}

public class CartPricingRequestDto
{
    public List<CartItemDto> Items { get; set; } = new();
}

public class CartPricingResultDto
{
    public List<CartLineDto> Lines { get; set; } = new();
    public decimal SubtotalUSD { get; set; }
    public decimal ShippingFeeUSD { get; set; }
    public decimal TotalUSD { get; set; }
    public List<string> Warnings { get; set; } = new();   // stock কম/নেই হলে এখানে জানাবে
}

public class CartLineDto
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int AvailableQty { get; set; }
    public decimal UnitPriceUSD { get; set; }
    public decimal LineTotalUSD { get; set; }
    public bool IsAvailable { get; set; }
}

public class CheckoutDto
{
    [Required, MinLength(1)] public List<CartItemDto> Items { get; set; } = new();

    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [Required, Phone] public string Phone { get; set; } = string.Empty;
    [EmailAddress] public string? Email { get; set; }

    [Required] public string ShippingAddress { get; set; } = string.Empty;
    [Required] public string ShippingCity { get; set; } = string.Empty;
    public string? ShippingNotes { get; set; }

    [Required] public string PaymentMethod { get; set; } = "COD";
    public string Currency { get; set; } = "USD";

    // লগইন করা customer হলে CustomerId server-side claim থেকে আসবে, dto তে দরকার নেই
}

public class OrderConfirmationDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalUSD { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? DigitalPaymentRedirectUrl { get; set; }   // bKash/Nagad হলে redirect লাগবে
}

// ── Customer Account (Online Login) ───────────────────────────────────────
public class CustomerRegisterDto
{
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [Required, Phone] public string Phone { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
}

public class CustomerLoginDto
{
    [Required] public string EmailOrPhone { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class MyOrderListDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalUSD { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

// ── Admin Order Management ────────────────────────────────────────────────
public class OnlineOrderListDto : MyOrderListDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}

public class OnlineOrderDetailDto : OnlineOrderListDto
{
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string? ShippingNotes { get; set; }
    public string? CancellationReason { get; set; }
    public List<OnlineOrderItemDto> Items { get; set; } = new();
}

public class OnlineOrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceUSD { get; set; }
    public decimal LineTotalUSD { get; set; }
}

public class UpdateOrderStatusDto
{
    [Required] public int OrderId { get; set; }
    [Required] public string Status { get; set; } = string.Empty;
    public string? CancellationReason { get; set; }
}

public class StorefrontSettingsDto
{
    public bool IsStoreEnabled { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreTagline { get; set; } = string.Empty;
    public decimal FlatShippingFeeUSD { get; set; }
    public decimal FreeShippingThresholdUSD { get; set; }
    public bool CodEnabled { get; set; }
    public bool BkashEnabled { get; set; }
    public bool NagadEnabled { get; set; }
    public int FulfillmentBranchId { get; set; }
    public string? AnnouncementText { get; set; }
}