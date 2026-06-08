namespace ClothingERP.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ContactPerson).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.BankName).HasMaxLength(200);
        builder.Property(x => x.BankAccountNumber).HasMaxLength(50);
        builder.Property(x => x.CurrentBalance).HasColumnType("decimal(18,2)");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class SupplierLedgerConfiguration : IEntityTypeConfiguration<SupplierLedger>
{
    public void Configure(EntityTypeBuilder<SupplierLedger> builder)
    {
        builder.ToTable("SupplierLedgers");
        builder.Property(x => x.Debit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Credit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Balance).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => x.SupplierId);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.LedgerEntries)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}