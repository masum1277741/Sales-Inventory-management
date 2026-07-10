namespace ClothingERP.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ImagePath).HasMaxLength(500);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SubCategoryConfiguration : IEntityTypeConfiguration<SubCategory>
{
    public void Configure(EntityTypeBuilder<SubCategory> builder)
    {
        builder.ToTable("SubCategories");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => new { x.Name, x.CategoryId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Category)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.LogoPath).HasMaxLength(500);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        builder.ToTable("Sizes");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> builder)
    {
        builder.ToTable("Colors");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.HexCode).IsRequired().HasMaxLength(7);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.SKU).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ImagePath).HasMaxLength(500);
        builder.Property(x => x.CostPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RetailPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.WholesalePrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SpecialPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxRate).HasColumnType("decimal(5,2)");
        builder.HasIndex(x => x.SKU).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Category)
            .WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SubCategory)
            .WithMany(x => x.Products).HasForeignKey(x => x.SubCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Brand)
            .WithMany(x => x.Products).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.Property(x => x.Barcode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.QRCode).HasMaxLength(500);
        builder.Property(x => x.CostPriceOverride).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RetailPriceOverride).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.Barcode).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.SizeId, x.ColorId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Product)
            .WithMany(x => x.Variants).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Size)
            .WithMany().HasForeignKey(x => x.SizeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Color)
            .WithMany().HasForeignKey(x => x.ColorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductBundleConfiguration : IEntityTypeConfiguration<ProductBundle>
{
    public void Configure(EntityTypeBuilder<ProductBundle> builder)
    {
        builder.ToTable("ProductBundles");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.BundlePrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ImagePath).HasMaxLength(500);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class ProductBundleItemConfiguration : IEntityTypeConfiguration<ProductBundleItem>
{
    public void Configure(EntityTypeBuilder<ProductBundleItem> builder)
    {
        builder.ToTable("ProductBundleItems");
        builder.HasIndex(x => new { x.ProductBundleId, x.ProductVariantId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.ProductBundle)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ProductBundleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}