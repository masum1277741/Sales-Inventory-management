namespace ClothingERP.Infrastructure.Data.Configurations;

public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.ToTable("SalesReturns");
        builder.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.ReturnNumber).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.SalesInvoice)
            .WithMany().HasForeignKey(x => x.SalesInvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer)
            .WithMany().HasForeignKey(x => x.CustomerId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalesReturnItemConfiguration : IEntityTypeConfiguration<SalesReturnItem>
{
    public void Configure(EntityTypeBuilder<SalesReturnItem> builder)
    {
        builder.ToTable("SalesReturnItems");
        builder.Property(x => x.ReturnQuantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DefectDescription).HasMaxLength(500);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.SalesReturn)
            .WithMany(x => x.Items).HasForeignKey(x => x.SalesReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductVariant)
            .WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ToTable("PurchaseReturns");
        builder.Property(x => x.ReturnNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => x.ReturnNumber).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.PurchaseOrder)
            .WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Supplier)
            .WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseReturnItemConfiguration : IEntityTypeConfiguration<PurchaseReturnItem>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
    {
        builder.ToTable("PurchaseReturnItems");
        builder.Property(x => x.ReturnQuantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DefectDescription).HasMaxLength(500);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.PurchaseReturn)
            .WithMany(x => x.Items).HasForeignKey(x => x.PurchaseReturnId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductVariant)
            .WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}