namespace ClothingERP.Application.Services;

public class StorefrontService : IStorefrontService
{
    private readonly IUnitOfWork _uow;

    public StorefrontService(IUnitOfWork uow) => _uow = uow;

    public async Task<StorefrontSettingsDto> GetSettingsAsync()
    {
        var s = (await _uow.StorefrontSettings.GetAllAsync()).FirstOrDefault();
        if (s == null)
        {
            var mainBranch = await _uow.Branches.GetQueryable().FirstOrDefaultAsync(b => b.IsMainBranch);
            s = new StorefrontSettings { FulfillmentBranchId = mainBranch?.Id ?? 1 };
            await _uow.StorefrontSettings.AddAsync(s);
            await _uow.SaveChangesAsync();
        }
        return new StorefrontSettingsDto
        {
            IsStoreEnabled = s.IsStoreEnabled,
            StoreName = s.StoreName,
            StoreTagline = s.StoreTagline,
            FlatShippingFeeUSD = s.FlatShippingFeeUSD,
            FreeShippingThresholdUSD = s.FreeShippingThresholdUSD,
            CodEnabled = s.CodEnabled,
            BkashEnabled = s.BkashEnabled,
            NagadEnabled = s.NagadEnabled,
            FulfillmentBranchId = s.FulfillmentBranchId,
            AnnouncementText = s.AnnouncementText
        };
    }

    public async Task<ServiceResult> UpdateSettingsAsync(StorefrontSettingsDto dto, int userId)
    {
        var s = (await _uow.StorefrontSettings.GetAllAsync()).FirstOrDefault();
        if (s == null) { s = new StorefrontSettings(); await _uow.StorefrontSettings.AddAsync(s); }

        s.IsStoreEnabled = dto.IsStoreEnabled; s.StoreName = dto.StoreName; s.StoreTagline = dto.StoreTagline;
        s.FlatShippingFeeUSD = dto.FlatShippingFeeUSD; s.FreeShippingThresholdUSD = dto.FreeShippingThresholdUSD;
        s.CodEnabled = dto.CodEnabled; s.BkashEnabled = dto.BkashEnabled; s.NagadEnabled = dto.NagadEnabled;
        s.FulfillmentBranchId = dto.FulfillmentBranchId; s.AnnouncementText = dto.AnnouncementText;
        s.UpdatedBy = userId; s.UpdatedAt = DateTime.UtcNow;

        _uow.StorefrontSettings.Update(s);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Storefront settings updated.");
    }

    // ── Product Listing (filters + pagination) ────────────────────────────
    public async Task<PagedResultDto<StorefrontProductDto>> GetProductsAsync(ProductFilterDto filter)
    {
        var settings = await GetSettingsAsync();

        var query = _uow.Products.GetQueryable()
            .Include(p => p.Category)
            .Include(p => p.Variants).ThenInclude(v => v.Size)
            .Include(p => p.Variants).ThenInclude(v => v.Color)
            .Include(p => p.Variants).ThenInclude(v => v.Stock)
            .Where(p => p.IsActive && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(p => p.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase));

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId);

        var allProducts = await query.ToListAsync();

        // ── Branch-specific stock (Feature #21 থাকলে) — fulfillment branch এর stock দেখাবে ──
        var mapped = allProducts.Select(p =>
        {
            var branchVariants = p.Variants.Where(v => v.IsActive);
            var prices = branchVariants.Select(v => v.RetailPriceOverride ?? p.RetailPrice).ToList();
            var stockSum = branchVariants.Sum(v =>
                v.Stock?.Quantity ?? 0);   // multi-branch হলে এখানে fulfillment branch filter যুক্ত করুন

            return new StorefrontProductDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryName = p.Category.Name,
                ImagePath = p.ImagePath,
                MinPriceUSD = prices.Any() ? prices.Min() : 0,
                MaxPriceUSD = prices.Any() ? prices.Max() : 0,
                InStock = stockSum > 0,
                AvailableSizes = branchVariants.Select(v => v.Size.Name).Distinct().ToList(),
                AvailableColors = branchVariants.Select(v => v.Color.Name).Distinct().ToList()
            };
        }).ToList();

        if (filter.MinPrice.HasValue) mapped = mapped.Where(p => p.MaxPriceUSD >= filter.MinPrice).ToList();
        if (filter.MaxPrice.HasValue) mapped = mapped.Where(p => p.MinPriceUSD <= filter.MaxPrice).ToList();

        mapped = filter.SortBy switch
        {
            "PriceLowHigh" => mapped.OrderBy(p => p.MinPriceUSD).ToList(),
            "PriceHighLow" => mapped.OrderByDescending(p => p.MinPriceUSD).ToList(),
            "Name" => mapped.OrderBy(p => p.Name).ToList(),
            _ => mapped.OrderByDescending(p => p.ProductId).ToList()  // Newest
        };

        var totalCount = mapped.Count;
        var paged = mapped.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList();

        return new PagedResultDto<StorefrontProductDto>
        {
            Items = paged,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<StorefrontProductDetailDto?> GetProductDetailAsync(int productId)
    {
        var p = await _uow.Products.GetQueryable()
            .Include(x => x.Category)
            .Include(x => x.Variants).ThenInclude(v => v.Size)
            .Include(x => x.Variants).ThenInclude(v => v.Color)
            .Include(x => x.Variants).ThenInclude(v => v.Stock)
            .FirstOrDefaultAsync(x => x.Id == productId && x.IsActive && !x.IsDeleted);

        if (p == null) return null;

        var activeVariants = p.Variants.Where(v => v.IsActive).ToList();
        var prices = activeVariants.Select(v => v.RetailPriceOverride ?? p.RetailPrice).ToList();

        return new StorefrontProductDetailDto
        {
            ProductId = p.Id,
            Name = p.Name,
            Description = p.Description,
            CategoryName = p.Category.Name,
            ImagePath = p.ImagePath,
            MinPriceUSD = prices.Any() ? prices.Min() : 0,
            MaxPriceUSD = prices.Any() ? prices.Max() : 0,
            InStock = activeVariants.Any(v => (v.Stock?.Quantity ?? 0) > 0),
            AvailableSizes = activeVariants.Select(v => v.Size.Name).Distinct().ToList(),
            AvailableColors = activeVariants.Select(v => v.Color.Name).Distinct().ToList(),
            Variants = activeVariants.Select(v => new StorefrontVariantDto
            {
                VariantId = v.Id,
                SizeName = v.Size.Name,
                ColorName = v.Color.Name,
                PriceUSD = v.RetailPriceOverride ?? p.RetailPrice,
                StockQty = (int)(v.Stock?.Quantity ?? 0),
                InStock = (v.Stock?.Quantity ?? 0) > 0
            }).ToList()
        };
    }

    public async Task<List<StorefrontProductDto>> GetFeaturedProductsAsync(int count = 8)
    {
        var result = await GetProductsAsync(new ProductFilterDto { Page = 1, PageSize = count, SortBy = "Newest" });
        return result.Items;
    }
}