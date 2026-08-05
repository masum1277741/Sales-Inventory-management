namespace ClothingERP.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // ── Auth ────────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AppModule> AppModules => Set<AppModule>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // ── Product ─────────────────────────────────────────────────────
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductBundle> ProductBundles => Set<ProductBundle>();
    public DbSet<ProductBundleItem> ProductBundleItems => Set<ProductBundleItem>();

    // ── Stock ───────────────────────────────────────────────────────
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // ── Customer ────────────────────────────────────────────────────
    public DbSet<CustomerGroup> CustomerGroups => Set<CustomerGroup>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerLedger> CustomerLedgers => Set<CustomerLedger>();
    public DbSet<CustomerPayment> CustomerPayments => Set<CustomerPayment>();

    // ── Loyalty ─────────────────────────────────────────────────────
    public DbSet<LoyaltySettings> LoyaltySettings => Set<LoyaltySettings>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();

    // ── Gift Card ───────────────────────────────────────────────────
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<GiftCardTransaction> GiftCardTransactions => Set<GiftCardTransaction>();

    // ── Commission ──────────────────────────────────────────────────
    public DbSet<CommissionSettings> CommissionSettings => Set<CommissionSettings>();
    public DbSet<StaffCommissionRate> StaffCommissionRates => Set<StaffCommissionRate>();
    public DbSet<CommissionTransaction> CommissionTransactions => Set<CommissionTransaction>();

    // ── Notification / Dashboard ───────────────────────────────────
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<DashboardLayout> DashboardLayouts => Set<DashboardLayout>();

    // ── Supplier ────────────────────────────────────────────────────
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierLedger> SupplierLedgers => Set<SupplierLedger>();

    // ── Purchase ────────────────────────────────────────────────────
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
    public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems => Set<GoodsReceiptNoteItem>();

    // ── Sales ───────────────────────────────────────────────────────
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<SalesPayment> SalesPayments => Set<SalesPayment>();

    // ── Returns ─────────────────────────────────────────────────────
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();

    // ── Accounts & Security ─────────────────────────────────────────
    public DbSet<AccountTransaction> AccountTransactions => Set<AccountTransaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    // ── Exchange Rate ─────────────────────────────────────────────── 
    public DbSet<ExchangeRateSettings> ExchangeRateSettings => Set<ExchangeRateSettings>();
    public DbSet<ExchangeRateSnapshot> ExchangeRateSnapshots => Set<ExchangeRateSnapshot>();
    public DbSet<ReorderSettings> ReorderSettings => Set<ReorderSettings>();
    public DbSet<ForecastSettings> ForecastSettings => Set<ForecastSettings>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<UserBranch> UserBranches => Set<UserBranch>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<OnlineOrder> onlineOrders => Set<OnlineOrder>();
    public DbSet<OnlineOrderItem> OnlineOrderItems => Set<OnlineOrderItem>();
    public DbSet<StorefrontSettings> StorefrontSettings => Set<StorefrontSettings>();
    public DbSet<DigitalPaymentTransaction> DigitalPaymentTransactions => Set<DigitalPaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // ── StockTransfer: prevent multiple cascade paths on Branch ──
        modelBuilder.Entity<StockTransfer>()
            .HasOne(st => st.FromBranch)
            .WithMany()
            .HasForeignKey(st => st.FromBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransfer>()
            .HasOne(st => st.ToBranch)
            .WithMany()
            .HasForeignKey(st => st.ToBranchId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Customer>()
            .Property(c => c.BranchId)
            .HasDefaultValue(1);

        modelBuilder.Entity<Product>()
            .Property(p => p.BranchId)
            .HasDefaultValue(1);

        modelBuilder.Entity<Supplier>()
            .Property(s => s.BranchId)
            .HasDefaultValue(1);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.IsDeleted = false;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}