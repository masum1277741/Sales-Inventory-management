namespace ClothingERP.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IBarcodeService _barcode;

    public ProductService(IUnitOfWork uow, IMapper mapper, IBarcodeService barcode)
        => (_uow, _mapper, _barcode) = (uow, mapper, barcode);

    public async Task<IEnumerable<ProductListDto>> GetAllAsync()
    {
        var list = await _uow.Products.GetWithDetailsAsync();
        return _mapper.Map<IEnumerable<ProductListDto>>(list);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _uow.Products.GetWithVariantsAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto, int userId)
    {
        await _uow.BeginTransactionAsync();
        try
        {
            var seq = await _uow.Products.GetNextSkuSequenceAsync();
            var category = await _uow.Categories.GetByIdAsync(dto.CategoryId);
            var sku = _barcode.GenerateSKU(category?.Name ?? "PRD", seq);

            var product = _mapper.Map<Product>(dto);
            product.SKU = sku;
            product.CreatedBy = userId;

            await _uow.Products.AddAsync(product);
            await _uow.SaveChangesAsync();

            foreach (var variantDto in dto.Variants)
            {
                var barcodeVal = _barcode.GenerateBarcode(sku, variantDto.SizeId, variantDto.ColorId);
                var variant = new ProductVariant
                {
                    ProductId = product.Id,
                    SizeId = variantDto.SizeId,
                    ColorId = variantDto.ColorId,
                    Barcode = barcodeVal,
                    CostPriceOverride = variantDto.CostPriceOverride,
                    RetailPriceOverride = variantDto.RetailPriceOverride,
                    IsActive = true,
                    CreatedBy = userId
                };
                await _uow.ProductVariants.AddAsync(variant);
                await _uow.SaveChangesAsync();

                // Initialize stock record
                await _uow.Stocks.AddAsync(new Stock
                {
                    ProductVariantId = variant.Id,
                    Quantity = 0,
                    ReservedQuantity = 0,
                    CreatedBy = userId
                });
            }
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();

            var result = await _uow.Products.GetWithVariantsAsync(product.Id);
            return ServiceResult<ProductDto>.Ok(_mapper.Map<ProductDto>(result!), "Product created.");
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync();
            return ServiceResult<ProductDto>.Fail($"Failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, int userId)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null) return ServiceResult<ProductDto>.Fail("Product not found.");

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
        product.UpdatedBy = userId;
        if (dto.ImagePath != null) product.ImagePath = dto.ImagePath;

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();

        var result = await _uow.Products.GetWithVariantsAsync(id);
        return ServiceResult<ProductDto>.Ok(_mapper.Map<ProductDto>(result!), "Product updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null) return ServiceResult.Fail("Not found.");
        _uow.Products.Remove(product);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }

    public async Task<ServiceResult> ToggleStatusAsync(int id, int userId)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product == null) return ServiceResult.Fail("Not found.");
        product.IsActive = !product.IsActive; product.UpdatedBy = userId;
        _uow.Products.Update(product); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Status toggled.");
    }

    public async Task<ProductVariantDto?> GetVariantByBarcodeAsync(string barcode)
    {
        var variant = await _uow.ProductVariants.GetByBarcodeAsync(barcode);
        return variant == null ? null : _mapper.Map<ProductVariantDto>(variant);
    }

    public async Task<IEnumerable<ProductVariantDto>> SearchVariantsAsync(string keyword)
    {
        var products = await _uow.Products.SearchAsync(keyword);
        var variants = products.SelectMany(p => p.Variants.Where(v => v.IsActive && !v.IsDeleted));
        return _mapper.Map<IEnumerable<ProductVariantDto>>(variants);
    }

    public async Task<ServiceResult<ProductVariantDto>> AddVariantAsync(int productId, CreateProductVariantDto dto, int userId)
    {
        var product = await _uow.Products.GetByIdAsync(productId);
        if (product == null) return ServiceResult<ProductVariantDto>.Fail("Product not found.");
        if (await _uow.ProductVariants.SizeColorCombinationExistsAsync(productId, dto.SizeId, dto.ColorId))
            return ServiceResult<ProductVariantDto>.Fail("This size-color combination already exists.");

        var barcodeVal = _barcode.GenerateBarcode(product.SKU, dto.SizeId, dto.ColorId);
        var variant = new ProductVariant
        {
            ProductId = productId,
            SizeId = dto.SizeId,
            ColorId = dto.ColorId,
            Barcode = barcodeVal,
            IsActive = true,
            CreatedBy = userId,
            CostPriceOverride = dto.CostPriceOverride,
            RetailPriceOverride = dto.RetailPriceOverride
        };
        await _uow.ProductVariants.AddAsync(variant);
        await _uow.SaveChangesAsync();
        await _uow.Stocks.AddAsync(new Stock { ProductVariantId = variant.Id, Quantity = 0, CreatedBy = userId });
        await _uow.SaveChangesAsync();

        var result = await _uow.ProductVariants.GetWithFullDetailsAsync(variant.Id);
        return ServiceResult<ProductVariantDto>.Ok(_mapper.Map<ProductVariantDto>(result!), "Variant added.");
    }

    public async Task<ServiceResult> DeleteVariantAsync(int variantId)
    {
        var variant = await _uow.ProductVariants.GetByIdAsync(variantId);
        if (variant == null) return ServiceResult.Fail("Not found.");
        _uow.ProductVariants.Remove(variant); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Variant deleted.");
    }

    public async Task<ServiceResult> RegenerateBarcodeAsync(int variantId, int userId)
    {
        var variant = await _uow.ProductVariants.GetWithFullDetailsAsync(variantId);
        if (variant == null) return ServiceResult.Fail("Not found.");
        variant.Barcode = _barcode.GenerateBarcode(variant.Product.SKU, variant.SizeId, variant.ColorId);
        variant.UpdatedBy = userId;
        _uow.ProductVariants.Update(variant); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Barcode regenerated.");
    }
}