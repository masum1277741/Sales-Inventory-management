namespace ClothingERP.Application.Services;

public class BundleService : IBundleService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public BundleService(IUnitOfWork uow, IMapper mapper)
        => (_uow, _mapper) = (uow, mapper);


    private async Task<(decimal regularPrice, int availableStock)> CalculateBundleMetricsAsync(ProductBundle bundle)
    {
        decimal regularPrice = 0;
        int availableStock = int.MaxValue;

        foreach (var item in bundle.Items)
        {
            var variant = await _uow.ProductVariants.GetByIdAsync(item.ProductVariantId);
            if (variant == null) continue;

            var unitPrice = variant.RetailPriceOverride ?? variant.Product.RetailPrice;
            regularPrice += unitPrice * item.Quantity;

            var stock = await _uow.Stocks.GetByVariantIdAsync(item.ProductVariantId);
            var qtyAvailable = stock?.Quantity ?? 0;
            var bundlesPossible = item.Quantity > 0 ? (int)(qtyAvailable / item.Quantity) : 0;
            availableStock = Math.Min(availableStock, bundlesPossible);
        }

        if (availableStock == int.MaxValue) availableStock = 0;
        return (regularPrice, availableStock);
    }

    // ── Get All ───────────────────────────────────────────────────────────
    public async Task<IEnumerable<ProductBundleListDto>> GetAllAsync()
    {
        var bundles = await _uow.ProductBundles.GetQueryable()
            .Include(b => b.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var result = new List<ProductBundleListDto>();
        foreach (var b in bundles)
        {
            var (regularPrice, stock) = await CalculateBundleMetricsAsync(b);
            var savings = regularPrice - b.BundlePrice;

            result.Add(new ProductBundleListDto
            {
                Id = b.Id,
                Name = b.Name,
                BundlePrice = b.BundlePrice,
                RegularPrice = regularPrice,
                SavingsAmount = savings,
                SavingsPercent = regularPrice > 0 ? (savings / regularPrice) * 100 : 0,
                ItemCount = b.Items.Count,
                AvailableStock = stock,
                IsActive = b.IsActive
            });
        }
        return result;
    }

    // ── Get By Id ─────────────────────────────────────────────────────────
    public async Task<ProductBundleDto?> GetByIdAsync(int id)
    {
        var bundle = await _uow.ProductBundles.GetQueryable()
            .Include(b => b.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(b => b.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Size)
            .Include(b => b.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

        if (bundle == null) return null;

        var (regularPrice, stock) = await CalculateBundleMetricsAsync(bundle);
        var savings = regularPrice - bundle.BundlePrice;

        var itemDtos = new List<ProductBundleItemDto>();
        foreach (var item in bundle.Items)
        {
            var v = item.ProductVariant;
            var stockRec = await _uow.Stocks.GetByVariantIdAsync(item.ProductVariantId);
            itemDtos.Add(new ProductBundleItemDto
            {
                Id = item.Id,
                ProductVariantId = item.ProductVariantId,
                ProductName = v.Product.Name,
                SizeName = v.Size.Name,
                ColorName = v.Color.Name,
                Barcode = v.Barcode,
                UnitPrice = v.RetailPriceOverride ?? v.Product.RetailPrice,
                Quantity = item.Quantity,
                AvailableStock = Convert.ToInt32( stockRec?.Quantity ?? 0)
            });
        }

        return new ProductBundleDto
        {
            Id = bundle.Id,
            Name = bundle.Name,
            Description = bundle.Description,
            ImagePath = bundle.ImagePath,
            BundlePrice = bundle.BundlePrice,
            RegularPrice = regularPrice,
            SavingsAmount = savings,
            SavingsPercent = regularPrice > 0 ? (savings / regularPrice) * 100 : 0,
            ItemCount = bundle.Items.Count,
            AvailableStock = stock,
            IsActive = bundle.IsActive,
            StartDate = bundle.StartDate,
            EndDate = bundle.EndDate,
            Items = itemDtos
        };
    }

    // ── Create ────────────────────────────────────────────────────────────
    public async Task<ServiceResult<ProductBundleDto>> CreateAsync(CreateProductBundleDto dto, int userId)
    {
        if (dto.Items.Count < 2)
            return ServiceResult<ProductBundleDto>.Fail("একটা bundle এ কমপক্ষে ২টা ভিন্ন item থাকতে হবে।");

        var bundle = new ProductBundle
        {
            Name = dto.Name,
            Description = dto.Description,
            BundlePrice = dto.BundlePrice,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive,
            CreatedBy = userId
        };
        await _uow.ProductBundles.AddAsync(bundle);
        await _uow.SaveChangesAsync();

        foreach (var item in dto.Items)
        {
            await _uow.ProductBundleItems.AddAsync(new ProductBundleItem
            {
                ProductBundleId = bundle.Id,
                ProductVariantId = item.ProductVariantId,
                Quantity = Math.Max(1, item.Quantity),
                CreatedBy = userId
            });
        }
        await _uow.SaveChangesAsync();

        var result = await GetByIdAsync(bundle.Id);
        return ServiceResult<ProductBundleDto>.Ok(result!, "Bundle created successfully.");
    }

    // ── Update ────────────────────────────────────────────────────────────
    public async Task<ServiceResult<ProductBundleDto>> UpdateAsync(int id, CreateProductBundleDto dto, int userId)
    {
        var bundle = await _uow.ProductBundles.GetByIdAsync(id);
        if (bundle == null) return ServiceResult<ProductBundleDto>.Fail("Bundle not found.");

        bundle.Name = dto.Name;
        bundle.Description = dto.Description;
        bundle.BundlePrice = dto.BundlePrice;
        bundle.StartDate = dto.StartDate;
        bundle.EndDate = dto.EndDate;
        bundle.IsActive = dto.IsActive;
        bundle.UpdatedBy = userId;
        bundle.UpdatedAt = DateTime.UtcNow;
        _uow.ProductBundles.Update(bundle);

        // পুরনো items সরিয়ে নতুন গুলো বসাও
        var existingItems = await _uow.ProductBundleItems.GetQueryable()
            .Where(i => i.ProductBundleId == id).ToListAsync();
        foreach (var ei in existingItems) _uow.ProductBundleItems.Remove(ei);
        await _uow.SaveChangesAsync();

        foreach (var item in dto.Items)
        {
            await _uow.ProductBundleItems.AddAsync(new ProductBundleItem
            {
                ProductBundleId = id,
                ProductVariantId = item.ProductVariantId,
                Quantity = Math.Max(1, item.Quantity),
                CreatedBy = userId
            });
        }
        await _uow.SaveChangesAsync();

        var result = await GetByIdAsync(id);
        return ServiceResult<ProductBundleDto>.Ok(result!, "Bundle updated successfully.");
    }

    // ── Delete / Toggle ───────────────────────────────────────────────────
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var bundle = await _uow.ProductBundles.GetByIdAsync(id);
        if (bundle == null) return ServiceResult.Fail("Bundle not found.");
        _uow.ProductBundles.Remove(bundle);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Bundle deleted successfully.");
    }

    public async Task<ServiceResult> ToggleStatusAsync(int id, int userId)
    {
        var bundle = await _uow.ProductBundles.GetByIdAsync(id);
        if (bundle == null) return ServiceResult.Fail("Bundle not found.");
        bundle.IsActive = !bundle.IsActive;
        bundle.UpdatedBy = userId;
        bundle.UpdatedAt = DateTime.UtcNow;
        _uow.ProductBundles.Update(bundle);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok($"Bundle {(bundle.IsActive ? "activated" : "deactivated")}.");
    }

    // ── Search (POS এর জন্য) ──────────────────────────────────────────────
    public async Task<IEnumerable<BundleSearchDto>> SearchBundlesAsync(string keyword)
    {
        var today = DateTime.UtcNow;
        var query = _uow.ProductBundles.GetQueryable()
            .Include(b => b.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(b => b.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Size)
            .Include(b => b.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Color)
            .Where(b => !b.IsDeleted && b.IsActive &&
                       (b.StartDate == null || b.StartDate <= today) &&
                       (b.EndDate == null || b.EndDate >= today));

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(b => b.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        var bundles = await query.ToListAsync();
        var result = new List<BundleSearchDto>();

        foreach (var b in bundles)
        {
            var (regularPrice, stock) = await CalculateBundleMetricsAsync(b);
            if (stock <= 0) continue; // stock না থাকলে POS এ দেখাবে না

            result.Add(new BundleSearchDto
            {
                Id = b.Id,
                Name = b.Name,
                BundlePrice = b.BundlePrice,
                RegularPrice = regularPrice,
                SavingsAmount = regularPrice - b.BundlePrice,
                AvailableStock = stock,
                ItemsSummary = b.Items.Select(i =>
                    $"{i.ProductVariant.Product.Name} ({i.ProductVariant.Size.Name}/{i.ProductVariant.Color.Name}) x{i.Quantity}").ToList()
            });
        }
        return result;
    }

    public async Task<int> GetAvailableStockAsync(int bundleId)
    {
        var bundle = await _uow.ProductBundles.GetQueryable()
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == bundleId);
        if (bundle == null) return 0;
        var (_, stock) = await CalculateBundleMetricsAsync(bundle);
        return stock;
    }
}