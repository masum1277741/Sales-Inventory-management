namespace ClothingERP.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // ── Auth ─────────────────────────────────────────────────────────
    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IAppModuleRepository? _appModules;
    private IRolePermissionRepository? _rolePermissions;

    // ── Product ──────────────────────────────────────────────────────
    private ICategoryRepository? _categories;
    private ISubCategoryRepository? _subCategories;
    private IRepository<Brand>? _brands;
    private IRepository<Size>? _sizes;
    private IRepository<Color>? _colors;
    private IProductRepository? _products;
    private IProductVariantRepository? _productVariants;


    // ── Stock ────────────────────────────────────────────────────────
    private IStockRepository? _stocks;
    private IRepository<StockMovement>? _stockMovements;

    // ── Customer ─────────────────────────────────────────────────────
    private IRepository<CustomerGroup>? _customerGroups;
    private ICustomerRepository? _customers;
    private ICustomerLedgerRepository? _customerLedgers;
    private IRepository<CustomerPayment>? _customerPayments;
    private IRepository<LoyaltySettings>? _loyaltySettings;
    private IRepository<LoyaltyTransaction>? _loyaltyTransactions;
    // ── Supplier ─────────────────────────────────────────────────────
    private ISupplierRepository? _suppliers;
    private ISupplierLedgerRepository? _supplierLedgers;

    // ── Purchase ─────────────────────────────────────────────────────
    private IPurchaseOrderRepository? _purchaseOrders;
    private IGoodsReceiptNoteRepository? _goodsReceiptNotes;

    // ── Sales ────────────────────────────────────────────────────────
    private ISalesInvoiceRepository? _salesInvoices;
    private IRepository<SalesPayment>? _salesPayments;
    private ISalesReturnRepository? _salesReturns;
    private IPurchaseReturnRepository? _purchaseReturns;
    public IRepository<GiftCard> GiftCards { get; }
    public IRepository<GiftCardTransaction> GiftCardTransactions { get; }
    // ── Accounts & Security ──────────────────────────────────────────
    private IAccountTransactionRepository? _accountTransactions;
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _loyaltySettings = new GenericRepository<LoyaltySettings>(_context);
        _loyaltyTransactions = new GenericRepository<LoyaltyTransaction>(_context);
        ProductBundles = new GenericRepository<ProductBundle>(_context);
        ProductBundleItems = new GenericRepository<ProductBundleItem>(_context);
        GiftCards = new GenericRepository<GiftCard>(_context);
        GiftCardTransactions = new GenericRepository<GiftCardTransaction>(_context);
        CommissionSettings = new GenericRepository<CommissionSettings>(_context);
        StaffCommissionRates = new GenericRepository<StaffCommissionRate>(_context);
        CommissionTransactions = new GenericRepository<CommissionTransaction>(_context);
        Notifications = new GenericRepository<Notification>(_context);
        DashboardLayouts = new GenericRepository<DashboardLayout>(_context);
    }

    // ── Properties ───────────────────────────────────────────────────
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public IAppModuleRepository AppModules => _appModules ??= new AppModuleRepository(_context);
    public IRolePermissionRepository RolePermissions => _rolePermissions ??= new RolePermissionRepository(_context);
    public IRepository<ProductBundle> ProductBundles { get; }
    public IRepository<ProductBundleItem> ProductBundleItems { get; }
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public ISubCategoryRepository SubCategories => _subCategories ??= new SubCategoryRepository(_context);
    public IRepository<Brand> Brands => _brands ??= new GenericRepository<Brand>(_context);
    public IRepository<Size> Sizes => _sizes ??= new GenericRepository<Size>(_context);
    public IRepository<Color> Colors => _colors ??= new GenericRepository<Color>(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public IProductVariantRepository ProductVariants => _productVariants ??= new ProductVariantRepository(_context);
    public IRepository<LoyaltySettings> LoyaltySettings => _loyaltySettings!;
    public IRepository<LoyaltyTransaction> LoyaltyTransactions => _loyaltyTransactions!;
    public IStockRepository Stocks => _stocks ??= new StockRepository(_context);
    public IRepository<CommissionSettings> CommissionSettings { get; }
    public IRepository<StaffCommissionRate> StaffCommissionRates { get; }
    public IRepository<CommissionTransaction> CommissionTransactions { get; }
    public IRepository<StockMovement> StockMovements => _stockMovements ??= new GenericRepository<StockMovement>(_context);
    public IRepository<CustomerPayment> CustomerPayments
    => _customerPayments ??= new GenericRepository<CustomerPayment>(_context);
    public IRepository<CustomerGroup> CustomerGroups => _customerGroups ??= new GenericRepository<CustomerGroup>(_context);
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);
    public ICustomerLedgerRepository CustomerLedgers => _customerLedgers ??= new CustomerLedgerRepository(_context);

    public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(_context);
    public ISupplierLedgerRepository SupplierLedgers => _supplierLedgers ??= new SupplierLedgerRepository(_context);

    public IPurchaseOrderRepository PurchaseOrders => _purchaseOrders ??= new PurchaseOrderRepository(_context);
    public IGoodsReceiptNoteRepository GoodsReceiptNotes => _goodsReceiptNotes ??= new GoodsReceiptNoteRepository(_context);
    public IRepository<DashboardLayout> DashboardLayouts { get; }

    public ISalesInvoiceRepository SalesInvoices => _salesInvoices ??= new SalesInvoiceRepository(_context);
    public IRepository<SalesPayment> SalesPayments => _salesPayments ??= new GenericRepository<SalesPayment>(_context);
    public ISalesReturnRepository SalesReturns => _salesReturns ??= new SalesReturnRepository(_context);
    public IPurchaseReturnRepository PurchaseReturns => _purchaseReturns ??= new PurchaseReturnRepository(_context);
    public IRepository<Notification> Notifications { get; }
    public IAccountTransactionRepository AccountTransactions => _accountTransactions ??= new AccountTransactionRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);

    // ── Transactions ─────────────────────────────────────────────────
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();
    public async Task CommitTransactionAsync() { await _transaction!.CommitAsync(); await _transaction.DisposeAsync(); _transaction = null; }
    public async Task RollbackTransactionAsync() { await _transaction!.RollbackAsync(); await _transaction.DisposeAsync(); _transaction = null; }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}