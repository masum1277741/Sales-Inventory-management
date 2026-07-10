namespace ClothingERP.Infrastructure.Data.Configurations;

public class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.ToTable("SalesInvoices");
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmountBDT).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmountMVR).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ExchangeRateBDT).HasPrecision(18, 6);
        builder.Property(x => x.ExchangeRateMVR).HasPrecision(18, 6);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.HasIndex(x => x.InvoiceDate);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Customer)
            .WithMany(x => x.SalesInvoices)
            .HasForeignKey(x => x.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalesInvoiceItemConfiguration : IEntityTypeConfiguration<SalesInvoiceItem>
{
    public void Configure(EntityTypeBuilder<SalesInvoiceItem> builder)
    {
        builder.ToTable("SalesInvoiceItems");
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.SalesInvoice)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ProductVariant)
            .WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalesPaymentConfiguration : IEntityTypeConfiguration<SalesPayment>
{
    public void Configure(EntityTypeBuilder<SalesPayment> builder)
    {
        builder.ToTable("SalesPayments");
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.HasIndex(x => x.SalesInvoiceId);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.SalesInvoice)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}