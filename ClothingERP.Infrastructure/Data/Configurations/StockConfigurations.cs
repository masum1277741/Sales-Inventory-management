namespace ClothingERP.Infrastructure.Data.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks");
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.ReservedQuantity).HasColumnType("decimal(18,3)");
        builder.HasIndex(x => x.ProductVariantId).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.ProductVariant)
            .WithOne(x => x.Stock)
            .HasForeignKey<Stock>(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.PreviousQuantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.NewQuantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasIndex(x => x.StockId);
        builder.HasIndex(x => x.MovementDate);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Stock)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}