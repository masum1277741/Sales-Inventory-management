namespace ClothingERP.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Roles
        if (!context.Roles.Any())
        {
            var adminRole = new Role { Name = "Administrator", Description = "Full access", IsActive = true };
            var managerRole = new Role { Name = "Manager", Description = "Management access", IsActive = true };
            var cashierRole = new Role { Name = "Cashier", Description = "POS and sales", IsActive = true };
            context.Roles.AddRange(adminRole, managerRole, cashierRole);
            await context.SaveChangesAsync();
        }

        // Admin User
        if (!context.Users.Any())
        {
            var adminRole = context.Roles.First(r => r.Name == "Administrator");
            context.Users.Add(new User
            {
                FullName = "System Administrator",
                Username = "admin",
                Email = "admin@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@123"),
                IsActive = true,
                RoleId = adminRole.Id
            });
            await context.SaveChangesAsync();
        }

        // AppModules (Menu)
        if (!context.AppModules.Any())
        {
            var modules = new List<AppModule>
            {
                new() { Name = "Dashboard",           Icon = "bi-speedometer2",     Controller = "Dashboard",  Action = "Index",       SortOrder = 1  },
                new() { Name = "Products",            Icon = "bi-box-seam",         Controller = "Product",    Action = "Index",       SortOrder = 2  },
                new() { Name = "Categories",          Icon = "bi-tags",             Controller = "Category",   Action = "Index",       SortOrder = 3  },
                new() { Name = "Brands",              Icon = "bi-bookmark-star",    Controller = "Brand",      Action = "Index",       SortOrder = 4  },
                new() { Name = "Sizes & Colors",      Icon = "bi-palette",          Controller = "Attribute",  Action = "Index",       SortOrder = 5  },
                new() { Name = "Stock Management",    Icon = "bi-archive",          Controller = "Stock",      Action = "Index",       SortOrder = 6  },
                new() { Name = "Stock Adjustment",    Icon = "bi-sliders",          Controller = "Stock",      Action = "Adjustment",  SortOrder = 7  },
                new() { Name = "Sales & POS",         Icon = "bi-cart3",            Controller = "Sales",      Action = "POS",         SortOrder = 8  },
                new() { Name = "Sales History",       Icon = "bi-receipt",          Controller = "Sales",      Action = "Index",       SortOrder = 9  },
                new() { Name = "Customers",           Icon = "bi-people",           Controller = "Customer",   Action = "Index",       SortOrder = 10 },
                
                new() { Name = "Suppliers",           Icon = "bi-truck",            Controller = "Supplier",   Action = "Index",       SortOrder = 12 },
                new() { Name = "Purchase Orders",     Icon = "bi-bag-plus",         Controller = "Purchase",   Action = "Index",       SortOrder = 13 },
                new() { Name = "Goods Receipt (GRN)", Icon = "bi-box-arrow-in-down",Controller = "Purchase",   Action = "GRN",         SortOrder = 14 },
                new() { Name = "Sales Returns",       Icon = "bi-arrow-return-left",Controller = "Return",     Action = "Sales",       SortOrder = 15 },
                new() { Name = "Purchase Returns",    Icon = "bi-arrow-return-right",Controller = "Return",    Action = "Purchase",    SortOrder = 16 },
                new() { Name = "Income & Expense",    Icon = "bi-cash-stack",       Controller = "Account",    Action = "Index",       SortOrder = 17 },
                new() { Name = "Profit & Loss",       Icon = "bi-graph-up-arrow",   Controller = "Report",     Action = "ProfitLoss",  SortOrder = 18 },
                new() { Name = "Reports",             Icon = "bi-file-earmark-bar-graph", Controller = "Report", Action = "Index",    SortOrder = 19 },
                new() { Name = "Users",               Icon = "bi-person-badge",     Controller = "User",       Action = "Index",       SortOrder = 20 },
                new() { Name = "Roles & Permissions", Icon = "bi-shield-lock",      Controller = "Role",       Action = "Index",       SortOrder = 21 },
                new() { Name = "Audit Log",           Icon = "bi-clock-history",    Controller = "AuditLog",   Action = "Index",       SortOrder = 22 },
                new() { Name = "Barcode Print",       Icon = "bi-upc-scan",         Controller = "Barcode",    Action = "Index",       SortOrder = 23 },
                new() { Name = "Loyalty Settings",    Icon = "bi-gift",             Controller = "Loyalty",    Action = "Settings",    SortOrder = 24 },
            };
            context.AppModules.AddRange(modules);
            await context.SaveChangesAsync();
        }

        // Admin Role → Full Permissions
        var admin = context.Roles.First(r => r.Name == "Administrator");
        if (!context.RolePermissions.Any(rp => rp.RoleId == admin.Id))
        {
            var allModules = context.AppModules.ToList();
            var permissions = allModules.Select(m => new RolePermission
            {
                RoleId = admin.Id,
                ModuleId = m.Id,
                CanView = true,
                CanInsert = true,
                CanUpdate = true,
                CanDelete = true,
                CanPrint = true,
                CanExport = true
            }).ToList();
            context.RolePermissions.AddRange(permissions);
            await context.SaveChangesAsync();
        }

        // Customer Groups
        if (!context.CustomerGroups.Any())
        {
            context.CustomerGroups.AddRange(
                new CustomerGroup { Name = "Walk-in", DiscountPercentage = 0, IsActive = true },
                new CustomerGroup { Name = "Retail", DiscountPercentage = 2, IsActive = true },
                new CustomerGroup { Name = "Wholesale", DiscountPercentage = 5, IsActive = true },
                new CustomerGroup { Name = "VIP", DiscountPercentage = 10, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // Sizes
        if (!context.Sizes.Any())
        {
            context.Sizes.AddRange(
                new Size { Name = "XS", SortOrder = 1, IsActive = true },
                new Size { Name = "S", SortOrder = 2, IsActive = true },
                new Size { Name = "M", SortOrder = 3, IsActive = true },
                new Size { Name = "L", SortOrder = 4, IsActive = true },
                new Size { Name = "XL", SortOrder = 5, IsActive = true },
                new Size { Name = "XXL", SortOrder = 6, IsActive = true },
                new Size { Name = "3XL", SortOrder = 7, IsActive = true },
                new Size { Name = "Free Size", SortOrder = 8, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // Colors
        if (!context.Colors.Any())
        {
            context.Colors.AddRange(
                new Color { Name = "Black", HexCode = "#000000", IsActive = true },
                new Color { Name = "White", HexCode = "#FFFFFF", IsActive = true },
                new Color { Name = "Red", HexCode = "#FF0000", IsActive = true },
                new Color { Name = "Blue", HexCode = "#0000FF", IsActive = true },
                new Color { Name = "Green", HexCode = "#008000", IsActive = true },
                new Color { Name = "Yellow", HexCode = "#FFFF00", IsActive = true },
                new Color { Name = "Gray", HexCode = "#808080", IsActive = true },
                new Color { Name = "Navy", HexCode = "#000080", IsActive = true },
                new Color { Name = "Brown", HexCode = "#A52A2A", IsActive = true },
                new Color { Name = "Pink", HexCode = "#FFC0CB", IsActive = true }
            );
            await context.SaveChangesAsync();
        }
    }
}