namespace ClothingERP.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IBarcodeService _barcode;

    public ProductService(IUnitOfWork uow, IMapper mapper, IBarcodeService barcode)
        => (_uow, _mapper, _barcode) = (uow, mapper, barcode);

    // ── Get All (List) ────────────────────────────────────────────────────
    public async Task<IEnumerable<ProductListDto>> GetAllAsync()
    {
        var products = await _uow.Products.GetAllWithDetailsAsync();
        return _mapper.Map<IEnumerable<ProductListDto>>(
            products.OrderByDescending(p => p.CreatedAt));
    }
    public async Task<IEnumerable<ProductVariantDto>> GetAllActiveVariantsAsync()
    {
        var variants = await _uow.ProductVariants.GetAllWithDetailsAsync();
        var result = new List<ProductVariantDto>();

        foreach (var v in variants.Where(v => v.IsActive &&
                                              !v.IsDeleted &&
                                              v.Product.IsActive))
        {
            var dto = _mapper.Map<ProductVariantDto>(v);
            var stock = await _uow.Stocks.GetByVariantIdAsync(v.Id);
            dto.StockQuantity = stock?.Quantity ?? 0;
            result.Add(dto);
        }

        return result.OrderBy(v => v.ProductName).ToList();
    }
    // ── Get By Id ─────────────────────────────────────────────────────────
    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _uow.Products.GetByIdWithDetailsAsync(id);
        if (product == null) return null;

        var dto = _mapper.Map<ProductDto>(product);

        foreach (var variant in dto.Variants)
        {
            var stock = await _uow.Stocks.GetByVariantIdAsync(variant.Id);
            variant.StockQuantity = stock?.Quantity ?? 0;
        }

        return dto;
    }

    // ── Create ────────────────────────────────────────────────────────────
    public async Task<ServiceResult<ProductDto>> CreateAsync(
        CreateProductDto dto, int createdBy)
    {
        // Unique SKU generate
        var sku = $"PRD-{DateTime.Now:yyMMddHHmmss}";

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            SKU = sku,
            CategoryId = dto.CategoryId,
            SubCategoryId = dto.SubCategoryId,
            BrandId = dto.BrandId,
            CostPrice = dto.CostPrice,
            RetailPrice = dto.RetailPrice,
            WholesalePrice = dto.WholesalePrice,
            SpecialPrice = dto.SpecialPrice,
            TaxRate = dto.TaxRate,
            ReorderPoint = dto.ReorderPoint,
            ImagePath = dto.ImagePath,
            IsActive = dto.IsActive,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };

        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync();

        // Variants
        if (dto.Variants?.Any() == true)
        {
            foreach (var varDto in dto.Variants)
            {
                var barcode = _barcode.GenerateBarcodeNumber();

                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    SizeId = varDto.SizeId,
                    ColorId = varDto.ColorId,
                    Barcode = barcode,
                    CostPriceOverride = varDto.CostPriceOverride,
                    RetailPriceOverride = varDto.RetailPriceOverride,
                    IsActive = true,
                    CreatedBy = createdBy,
                    UpdatedBy = createdBy
                };

                await _uow.ProductVariants.AddAsync(variant);
                await _uow.SaveChangesAsync();

                // Initial stock record (0)
                var stock = new Stock
                {
                    ProductVariantId = variant.Id,
                    Quantity = 0,
                    CreatedBy = createdBy,
                    UpdatedBy = createdBy
                };
                await _uow.Stocks.AddAsync(stock);
                await _uow.SaveChangesAsync();
            }
        }

        return ServiceResult<ProductDto>.Ok(
            _mapper.Map<ProductDto>(product),
            "Product created successfully.");
    }

    // ── Bulk Price Update ──────────────────────────────────────────────────
    public async Task<BulkActionResultDto> BulkUpdatePriceAsync(BulkPriceUpdateDto dto, int userId)
    {
        var result = new BulkActionResultDto();

        var products = await _uow.Products.GetQueryable()
            .Where(p => dto.ProductIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        foreach (var product in products)
        {
            try
            {
                decimal ApplyChange(decimal current)
                {
                    decimal delta = dto.Mode == "Percent" ? current * (dto.Value / 100m) : dto.Value;
                    var newVal = dto.Direction == "Increase" ? current + delta : current - delta;
                    return Math.Max(0, Math.Round(newVal, 2));
                }

                if (dto.PriceField is "RetailPrice" or "Both")
                    product.RetailPrice = ApplyChange(product.RetailPrice);

                if (dto.PriceField is "CostPrice" or "Both")
                    product.CostPrice = ApplyChange(product.CostPrice);

                product.UpdatedBy = userId;
                product.UpdatedAt = DateTime.UtcNow;
                _uow.Products.Update(product);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailCount++;
                result.Errors.Add($"{product.Name}: {ex.Message}");
            }
        }

        await _uow.SaveChangesAsync();
        result.Success = result.SuccessCount > 0;
        result.Message = $"{result.SuccessCount} product(s) updated successfully" +
                          (result.FailCount > 0 ? $", {result.FailCount} failed." : ".");
        return result;
    }

    // ── Bulk Status Toggle ─────────────────────────────────────────────────
    public async Task<BulkActionResultDto> BulkToggleStatusAsync(BulkStatusUpdateDto dto, int userId)
    {
        var result = new BulkActionResultDto();
        var products = await _uow.Products.GetQueryable()
            .Where(p => dto.Ids.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        foreach (var product in products)
        {
            product.IsActive = dto.IsActive;
            product.UpdatedBy = userId;
            product.UpdatedAt = DateTime.UtcNow;
            _uow.Products.Update(product);
            result.SuccessCount++;
        }

        await _uow.SaveChangesAsync();
        result.Success = true;
        result.Message = $"{result.SuccessCount} product(s) {(dto.IsActive ? "activated" : "deactivated")}.";
        return result;
    }

    // ── Bulk Delete (soft delete) ──────────────────────────────────────────
    public async Task<BulkActionResultDto> BulkDeleteAsync(BulkDeleteDto dto)
    {
        var result = new BulkActionResultDto();
        var products = await _uow.Products.GetQueryable()
            .Where(p => dto.Ids.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        foreach (var product in products)
        {
            try
            {
                _uow.Products.Remove(product); // soft delete (IsDeleted = true via base repository)
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailCount++;
                result.Errors.Add($"{product.Name}: {ex.Message}");
            }
        }

        await _uow.SaveChangesAsync();
        result.Success = result.SuccessCount > 0;
        result.Message = $"{result.SuccessCount} product(s) deleted" +
                          (result.FailCount > 0 ? $", {result.FailCount} failed." : ".");
        return result;
    }

    // ── Bulk Category/Brand Reassign ───────────────────────────────────────
    public async Task<BulkActionResultDto> BulkUpdateCategoryAsync(BulkCategoryUpdateDto dto, int userId)
    {
        var result = new BulkActionResultDto();
        var products = await _uow.Products.GetQueryable()
            .Where(p => dto.ProductIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        foreach (var product in products)
        {
            if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;
            if (dto.SubCategoryId.HasValue) product.SubCategoryId = dto.SubCategoryId.Value;
            if (dto.BrandId.HasValue) product.BrandId = dto.BrandId.Value;
            product.UpdatedBy = userId;
            product.UpdatedAt = DateTime.UtcNow;
            _uow.Products.Update(product);
            result.SuccessCount++;
        }

        await _uow.SaveChangesAsync();
        result.Success = true;
        result.Message = $"{result.SuccessCount} product(s) re-categorized successfully.";
        return result;
    }

    // ── Update ────────────────────────────────────────────────────────────
    public async Task<ServiceResult<ProductDto>> UpdateAsync(
        int id, UpdateProductDto dto, int updatedBy)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null)
            return ServiceResult<ProductDto>.Fail("Product not found.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;
        product.SubCategoryId = dto.SubCategoryId;
        product.BrandId = dto.BrandId;
        product.CostPrice = dto.CostPrice;
        product.RetailPrice = dto.RetailPrice;
        product.WholesalePrice = dto.WholesalePrice;
        product.SpecialPrice = dto.SpecialPrice;
        product.TaxRate = dto.TaxRate;
        product.ReorderPoint = dto.ReorderPoint;
        product.IsActive = dto.IsActive;
        product.UpdatedBy = updatedBy;
        product.UpdatedAt = DateTime.UtcNow;

     
        if (!string.IsNullOrEmpty(dto.ImagePath))
            product.ImagePath = dto.ImagePath;

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();

        return ServiceResult<ProductDto>.Ok(
            _mapper.Map<ProductDto>(product),
            "Product updated successfully.");
    }

    // ── Delete ────────────────────────────────────────────────────────────
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null)
            return ServiceResult.Fail("Product not found.");

        // Soft delete
        _uow.Products.Remove(product);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok("Product deleted successfully.");
    }

    // ── Toggle Status ─────────────────────────────────────────────────────
    public async Task<ServiceResult> ToggleStatusAsync(int id, int updatedBy)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null)
            return ServiceResult.Fail("Product not found.");

        product.IsActive = !product.IsActive;
        product.UpdatedBy = updatedBy;
        product.UpdatedAt = DateTime.UtcNow;

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();

        var status = product.IsActive ? "activated" : "deactivated";
        return ServiceResult.Ok($"Product {status} successfully.");
    }

    // ── Add Variant ───────────────────────────────────────────────────────
    public async Task<ServiceResult<ProductVariantDto>> AddVariantAsync(
        int productId, CreateProductVariantDto dto, int createdBy)
    {
        // Duplicate check
        var existing = (await _uow.ProductVariants.GetAllAsync())
            .FirstOrDefault(v => v.ProductId == productId &&
                                 v.SizeId == dto.SizeId &&
                                 v.ColorId == dto.ColorId &&
                                 !v.IsDeleted);

        if (existing != null)
            return ServiceResult<ProductVariantDto>.Fail(
                "This Size + Color combination already exists.");

        var barcode = _barcode.GenerateBarcodeNumber();

        var variant = new ProductVariant
        {
            ProductId = productId,
            SizeId = dto.SizeId,
            ColorId = dto.ColorId,
            Barcode = barcode,
            CostPriceOverride = dto.CostPriceOverride,
            RetailPriceOverride = dto.RetailPriceOverride,
            IsActive = true,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };

        await _uow.ProductVariants.AddAsync(variant);
        await _uow.SaveChangesAsync();

        // Stock record
        var stock = new Stock
        {
            ProductVariantId = variant.Id,
            Quantity = 0,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
        await _uow.Stocks.AddAsync(stock);
        await _uow.SaveChangesAsync();

        return ServiceResult<ProductVariantDto>.Ok(
            _mapper.Map<ProductVariantDto>(variant),
            "Variant added successfully.");
    }

    // ── Delete Variant ────────────────────────────────────────────────────
    public async Task<ServiceResult> DeleteVariantAsync(int variantId)
    {
        var variant = await _uow.ProductVariants.GetByIdAsync(variantId);
        if (variant == null)
            return ServiceResult.Fail("Variant not found.");

        // Stock record আগে delete
        var stock = await _uow.Stocks.GetByVariantIdAsync(variantId);
        if (stock != null)
        {
            _uow.Stocks.Remove(stock);
            await _uow.SaveChangesAsync();
        }

        _uow.ProductVariants.Remove(variant);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok("Variant deleted successfully.");
    }

    // ── Regenerate Barcode ────────────────────────────────────────────────
    public async Task<ServiceResult> RegenerateBarcodeAsync(int variantId, int updatedBy)
    {
        var variant = await _uow.ProductVariants.GetByIdAsync(variantId);
        if (variant == null)
            return ServiceResult.Fail("Variant not found.");

        variant.Barcode = _barcode.GenerateBarcodeNumber();
        variant.UpdatedBy = updatedBy;
        variant.UpdatedAt = DateTime.UtcNow;

        _uow.ProductVariants.Update(variant);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok($"Barcode regenerated: {variant.Barcode}");
    }

    // ── Get Variant By Barcode ─────────────────────────────────────────────
    // ✅ Return type: ProductVariantDto? (interface match)
    public async Task<ProductVariantDto?> GetVariantByBarcodeAsync(string barcode)
    {
        var variants = await _uow.ProductVariants.GetAllWithDetailsAsync();
        var v = variants.FirstOrDefault(x => x.Barcode == barcode && x.IsActive);
        if (v == null) return null;

        var dto = _mapper.Map<ProductVariantDto>(v);

        // Stock populate
        var stock = await _uow.Stocks.GetByVariantIdAsync(v.Id);
        dto.StockQuantity = stock?.Quantity ?? 0;

        return dto;
    }

    // ── Search Variants ───────────────────────────────────────────────────
    // ✅ Return type: IEnumerable<ProductVariantDto> (interface match)
    public async Task<IEnumerable<ProductVariantDto>> SearchVariantsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Enumerable.Empty<ProductVariantDto>();

        var variants = await _uow.ProductVariants.GetAllWithDetailsAsync();

        var matched = variants
            .Where(v => v.IsActive &&
                        v.Product.IsActive &&
                        !v.IsDeleted &&
                       (v.Product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        v.Product.SKU.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        v.Barcode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        v.Size.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        v.Color.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var result = new List<ProductVariantDto>();

        foreach (var v in matched)
        {
            var dto = _mapper.Map<ProductVariantDto>(v);
            var stock = await _uow.Stocks.GetByVariantIdAsync(v.Id);
            dto.StockQuantity = stock?.Quantity ?? 0;
            result.Add(dto);
        }

        return result;
    }
}