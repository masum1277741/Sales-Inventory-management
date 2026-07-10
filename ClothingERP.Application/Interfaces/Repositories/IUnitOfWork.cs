namespace ClothingERP.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    // --- Auth & Users ---
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IAppModuleRepository AppModules { get; }
    IRolePermissionRepository RolePermissions { get; }

    // --- Products ---
    ICategoryRepository Categories { get; }
    ISubCategoryRepository SubCategories { get; }
    IRepository<Brand> Brands { get; }
    IRepository<Size> Sizes { get; }
    IRepository<Color> Colors { get; }
    IProductRepository Products { get; }
    IProductVariantRepository ProductVariants { get; }
    IRepository<LoyaltySettings> LoyaltySettings { get; }
    IRepository<LoyaltyTransaction> LoyaltyTransactions { get; }
    IRepository<ProductBundle> ProductBundles { get; }
    IRepository<ProductBundleItem> ProductBundleItems { get; }
    IRepository<GiftCard> GiftCards { get; }
    IRepository<GiftCardTransaction> GiftCardTransactions { get; }
    // --- Stock ---
    IStockRepository Stocks { get; }
    IRepository<StockMovement> StockMovements { get; }

    // --- Customers ---
    IRepository<CustomerGroup> CustomerGroups { get; }
    ICustomerRepository Customers { get; }
    ICustomerLedgerRepository CustomerLedgers { get; }
    IRepository<CustomerPayment> CustomerPayments { get; }

    // --- Suppliers ---
    ISupplierRepository Suppliers { get; }
    ISupplierLedgerRepository SupplierLedgers { get; }

    // --- Purchase ---
    IPurchaseOrderRepository PurchaseOrders { get; }
    IGoodsReceiptNoteRepository GoodsReceiptNotes { get; }

    // --- Sales ---
    ISalesInvoiceRepository SalesInvoices { get; }
    IRepository<SalesPayment> SalesPayments { get; }
    ISalesReturnRepository SalesReturns { get; }
    IPurchaseReturnRepository PurchaseReturns { get; }
    IRepository<CommissionSettings> CommissionSettings { get; }
    IRepository<StaffCommissionRate> StaffCommissionRates { get; }
    IRepository<CommissionTransaction> CommissionTransactions { get; }
    IRepository<DashboardLayout> DashboardLayouts { get; }
    // --- Accounts ---
    IAccountTransactionRepository AccountTransactions { get; }
    IRepository<Notification> Notifications { get; }
    // --- Security ---
    IAuditLogRepository AuditLogs { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}