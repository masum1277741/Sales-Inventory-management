namespace ClothingERP.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── User ─────────────────────────────────────────────────────────
        CreateMap<User, UserDto>()
            .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role != null ? s.Role.Name : string.Empty));
        CreateMap<CreateUserDto, User>()
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.ProfileImage, o => o.Ignore());

        // ── Role ─────────────────────────────────────────────────────────
        CreateMap<Role, RoleDto>()
            .ForMember(d => d.UserCount, o => o.MapFrom(s => s.Users.Count(u => !u.IsDeleted)));
        CreateMap<CreateRoleDto, Role>();
        CreateMap<RolePermission, RolePermissionDto>()
            .ForMember(d => d.ModuleName, o => o.MapFrom(s => s.Module.Name))
            .ForMember(d => d.ModuleIcon, o => o.MapFrom(s => s.Module.Icon))
            .ForMember(d => d.ParentModuleId, o => o.MapFrom(s => s.Module.ParentModuleId));

        // ── Category ─────────────────────────────────────────────────────
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.SubCategoryCount, o => o.MapFrom(s => s.SubCategories.Count(x => !x.IsDeleted)));
        CreateMap<CreateCategoryDto, Category>();

        // ── SubCategory ───────────────────────────────────────────────────
        CreateMap<SubCategory, SubCategoryDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name));
        CreateMap<CreateSubCategoryDto, SubCategory>();

        // ── Brand ─────────────────────────────────────────────────────────
        CreateMap<Brand, BrandDto>()
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count(p => !p.IsDeleted)));
        CreateMap<CreateBrandDto, Brand>();

        // ── Size & Color ──────────────────────────────────────────────────
        CreateMap<Size, SizeDto>();
        CreateMap<CreateSizeDto, Size>();
        CreateMap<Color, ColorDto>();
        CreateMap<CreateColorDto, Color>();

        // ── Product ───────────────────────────────────────────────────────
        CreateMap<Product, ProductListDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SubCategoryName, o => o.MapFrom(s => s.SubCategory.Name))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand.Name))
            .ForMember(d => d.VariantCount, o => o.MapFrom(s => s.Variants.Count(v => !v.IsDeleted)));

        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SubCategoryName, o => o.MapFrom(s => s.SubCategory.Name))
            .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand.Name));

        CreateMap<CreateProductDto, Product>()
            .ForMember(d => d.SKU, o => o.Ignore())
            .ForMember(d => d.ImagePath, o => o.Ignore())
            .ForMember(d => d.Variants, o => o.Ignore());

        CreateMap<LoyaltySettings, LoyaltySettingsDto>();
        // ── ProductVariant ────────────────────────────────────────────────

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(d => d.ProductName,
                o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
            .ForMember(d => d.ProductSKU,
                o => o.MapFrom(s => s.Product != null ? s.Product.SKU : string.Empty))
            .ForMember(d => d.SizeName,
                o => o.MapFrom(s => s.Size != null ? s.Size.Name : string.Empty))
            .ForMember(d => d.ColorName,
                o => o.MapFrom(s => s.Color != null ? s.Color.Name : string.Empty))
            .ForMember(d => d.ColorHex,
                o => o.MapFrom(s => s.Color != null ? s.Color.HexCode : string.Empty))
            .ForMember(d => d.EffectiveCostPrice,
                o => o.MapFrom(s => s.CostPriceOverride ?? (s.Product != null ? s.Product.CostPrice : 0)))
            .ForMember(d => d.EffectiveRetailPrice,
                o => o.MapFrom(s => s.RetailPriceOverride ?? (s.Product != null ? s.Product.RetailPrice : 0)))
            .ForMember(d => d.StockQuantity,
                o => o.MapFrom(s => s.Stock != null ? s.Stock.Quantity : 0));

        // ── Stock ─────────────────────────────────────────────────────────
        CreateMap<Stock, StockListDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductVariant.Product.Name))
            .ForMember(d => d.ProductSKU, o => o.MapFrom(s => s.ProductVariant.Product.SKU))
            .ForMember(d => d.SizeName, o => o.MapFrom(s => s.ProductVariant.Size.Name))
            .ForMember(d => d.ColorName, o => o.MapFrom(s => s.ProductVariant.Color.Name))
            .ForMember(d => d.Barcode, o => o.MapFrom(s => s.ProductVariant.Barcode))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.ProductVariant.Product.Category.Name))
            .ForMember(d => d.ReorderPoint, o => o.MapFrom(s => (decimal)s.ProductVariant.Product.ReorderPoint))
            .ForMember(d => d.StockValue, o => o.MapFrom(s => s.Quantity * (s.ProductVariant.CostPriceOverride ?? s.ProductVariant.Product.CostPrice)))
            .ForMember(d => d.Status, o => o.MapFrom(s =>
                s.Quantity <= 0 ? "Out of Stock" :
                s.Quantity <= s.ProductVariant.Product.ReorderPoint ? "Low Stock" : "In Stock"));

        CreateMap<StockMovement, StockMovementDto>()
            .ForMember(d => d.MovementType, o => o.MapFrom(s => s.MovementType.ToString()));

        // ── Customer ──────────────────────────────────────────────────────
        CreateMap<Customer, CustomerListDto>()
            .ForMember(d => d.GroupName, o => o.MapFrom(s => s.CustomerGroup.Name));
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.GroupName, o => o.MapFrom(s => s.CustomerGroup.Name));
        CreateMap<CreateCustomerDto, Customer>()
            .ForMember(d => d.ProfileImage, o => o.MapFrom(s => s.ProfileImagePath));
        CreateMap<CustomerGroup, CustomerGroupDto>()
            .ForMember(d => d.CustomerCount, o => o.MapFrom(s => s.Customers.Count(c => !c.IsDeleted)));
        CreateMap<CustomerLedger, CustomerLedgerDto>()
            .ForMember(d => d.EntryType, o => o.MapFrom(s => s.EntryType.ToString()));

        // ── Supplier ──────────────────────────────────────────────────────
        CreateMap<Supplier, SupplierListDto>();
        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierDto, Supplier>();
        CreateMap<SupplierLedger, SupplierLedgerDto>()
            .ForMember(d => d.EntryType, o => o.MapFrom(s => s.EntryType.ToString()));

        // ── PurchaseOrder ────────────────────────────────────────────────
        CreateMap<PurchaseOrder, PurchaseOrderListDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.CompanyName))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.DueAmount, o => o.MapFrom(s => s.TotalAmount - s.PaidAmount));
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.CompanyName))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.DueAmount, o => o.MapFrom(s => s.TotalAmount - s.PaidAmount));
        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductVariant.Product.Name))
            .ForMember(d => d.SizeName, o => o.MapFrom(s => s.ProductVariant.Size.Name))
            .ForMember(d => d.ColorName, o => o.MapFrom(s => s.ProductVariant.Color.Name))
            .ForMember(d => d.Barcode, o => o.MapFrom(s => s.ProductVariant.Barcode));

        // ── GRN ──────────────────────────────────────────────────────────
        CreateMap<GoodsReceiptNote, GRNListDto>()
            .ForMember(d => d.PONumber, o => o.MapFrom(s => s.PurchaseOrder.PONumber))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.CompanyName))
            .ForMember(d => d.PurchaseOrderId, o => o.MapFrom(s => s.PurchaseOrderId))
            .ForMember(d => d.TotalValue, o => o.MapFrom(s => s.Items.Sum(i => i.TotalCost)));
        CreateMap<GoodsReceiptNote, GRNDto>()
            .ForMember(d => d.PONumber, o => o.MapFrom(s => s.PurchaseOrder.PONumber))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.CompanyName))
            .ForMember(d => d.TotalValue, o => o.MapFrom(s => s.Items.Sum(i => i.TotalCost)));
        CreateMap<GoodsReceiptNoteItem, GRNItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductVariant.Product.Name))
            .ForMember(d => d.SizeName, o => o.MapFrom(s => s.ProductVariant.Size.Name))
            .ForMember(d => d.ColorName, o => o.MapFrom(s => s.ProductVariant.Color.Name))
            .ForMember(d => d.Barcode, o => o.MapFrom(s => s.ProductVariant.Barcode));

        // ── SalesInvoice ─────────────────────────────────────────────────
        CreateMap<SalesInvoice, SalesInvoiceListDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : "Walk-in Customer"))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.DueAmount, o => o.MapFrom(s => s.TotalAmount - s.PaidAmount));
        CreateMap<SalesInvoice, SalesInvoiceDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : "Walk-in Customer"))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.DueAmount, o => o.MapFrom(s => s.TotalAmount - s.PaidAmount));
        CreateMap<SalesInvoiceItem, SalesInvoiceItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductVariant.Product.Name))
            .ForMember(d => d.SKU, o => o.MapFrom(s => s.ProductVariant.Product.SKU))
            .ForMember(d => d.SizeName, o => o.MapFrom(s => s.ProductVariant.Size.Name))
            .ForMember(d => d.ColorName, o => o.MapFrom(s => s.ProductVariant.Color.Name))
            .ForMember(d => d.Barcode, o => o.MapFrom(s => s.ProductVariant.Barcode));
        CreateMap<SalesPayment, SalesPaymentDto>()
            .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()));

        // ── SalesReturn ───────────────────────────────────────────────────
        CreateMap<SalesReturn, SalesReturnListDto>()
            .ForMember(d => d.InvoiceNumber, o => o.MapFrom(s => s.SalesInvoice.InvoiceNumber))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : "Walk-in Customer"))
            .ForMember(d => d.ReturnType, o => o.MapFrom(s => s.ReturnType.ToString()));
        CreateMap<SalesReturn, SalesReturnDto>()
            .ForMember(d => d.InvoiceNumber, o => o.MapFrom(s => s.SalesInvoice.InvoiceNumber))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : "Walk-in Customer"))
            .ForMember(d => d.ReturnType, o => o.MapFrom(s => s.ReturnType.ToString()))
            .ForMember(d => d.RefundMethod, o => o.MapFrom(s => s.RefundMethod.ToString()));
        CreateMap<SalesReturnItem, SalesReturnItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductVariant.Product.Name))
            .ForMember(d => d.SizeName, o => o.MapFrom(s => s.ProductVariant.Size.Name))
            .ForMember(d => d.ColorName, o => o.MapFrom(s => s.ProductVariant.Color.Name));

        // ── PurchaseReturn ────────────────────────────────────────────────
        CreateMap<PurchaseReturn, PurchaseReturnListDto>()
            .ForMember(d => d.PONumber, o => o.MapFrom(s => s.PurchaseOrder.PONumber))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.CompanyName));
        CreateMap<PurchaseReturn, PurchaseReturnDto>()
            .ForMember(d => d.PONumber, o => o.MapFrom(s => s.PurchaseOrder.PONumber))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.CompanyName));
        CreateMap<PurchaseReturnItem, PurchaseReturnItemDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductVariant.Product.Name))
            .ForMember(d => d.SizeName, o => o.MapFrom(s => s.ProductVariant.Size.Name));

        // AccountTransaction → AccountTransactionListDto
        CreateMap<AccountTransaction, AccountTransactionListDto>()
            .ForMember(d => d.TransactionType,
                o => o.MapFrom(s => s.TransactionType.ToString()))
            .ForMember(d => d.Category,
                o => o.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.PaymentMethod,
                o => o.MapFrom(s => s.PaymentMethod.ToString()));
        CreateMap<AccountTransaction, AccountTransactionDto>()
            .ForMember(d => d.TransactionType, o => o.MapFrom(s => s.TransactionType.ToString()))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()));
        CreateMap<CreateAccountTransactionDto, AccountTransaction>()
            .ForMember(d => d.TransactionNumber, o => o.Ignore());
        CreateMap<CommissionSettings, CommissionSettingsDto>();
        // ── AuditLog ──────────────────────────────────────────────────────
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName));
    }
}