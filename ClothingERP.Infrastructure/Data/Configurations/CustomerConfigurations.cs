namespace ClothingERP.Infrastructure.Data.Configurations;

public class CustomerGroupConfiguration : IEntityTypeConfiguration<CustomerGroup>
{
    public void Configure(EntityTypeBuilder<CustomerGroup> builder)
    {
        builder.ToTable("CustomerGroups");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DiscountPercentage).HasColumnType("decimal(5,2)");
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.NIDNumber).HasMaxLength(50);
        builder.Property(x => x.ProfileImage).HasMaxLength(500);
        builder.Property(x => x.LoyaltyPoints).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalPurchaseAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CurrentBalance).HasColumnType("decimal(18,2)");
        builder.Property(x => x.BranchId).HasDefaultValue(1);
        builder.HasIndex(x => x.PhoneNumber);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.CustomerGroup)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.CustomerGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class CustomerPaymentConfiguration : IEntityTypeConfiguration<CustomerPayment>
{
    public void Configure(EntityTypeBuilder<CustomerPayment> builder)
    {
        builder.ToTable("CustomerPayments");
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class CustomerLedgerConfiguration : IEntityTypeConfiguration<CustomerLedger>
{
    public void Configure(EntityTypeBuilder<CustomerLedger> builder)
    {
        builder.ToTable("CustomerLedgers");
        builder.Property(x => x.Debit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Credit).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Balance).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.EntryDate);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasOne(x => x.Customer)
            .WithMany(x => x.LedgerEntries)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}