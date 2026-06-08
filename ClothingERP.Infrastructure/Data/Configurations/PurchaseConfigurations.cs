namespace ClothingERP.Infrastructure.Data.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.Property(x => x.PONumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ShippingCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => x.PONumber).IsUnique();
        builder.HasIndex(x => x.OrderDate);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.PurchaseOrders)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.Property(x => x.OrderedQuantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.ReceivedQuantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductVariant)
            .WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GoodsReceiptNoteConfiguration : IEntityTypeConfiguration<GoodsReceiptNote>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNote> builder)
    {
        builder.ToTable("GoodsReceiptNotes");
        builder.Property(x => x.GRNNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DeliveryChallan).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => x.GRNNumber).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.GRNs)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Supplier)
            .WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class GoodsReceiptNoteItemConfiguration : IEntityTypeConfiguration<GoodsReceiptNoteItem>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNoteItem> builder)
    {
        builder.ToTable("GoodsReceiptNoteItems");
        builder.Property(x => x.ReceivedQuantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCost).HasColumnType("decimal(18,2)");
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.GoodsReceiptNote)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.GoodsReceiptNoteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductVariant)
            .WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PurchaseOrderItem)
            .WithMany().HasForeignKey(x => x.PurchaseOrderItemId).OnDelete(DeleteBehavior.Restrict);
    }
}