namespace ClothingERP.Application.Constants;

public static class DashboardWidgetRegistry
{
    // সব available widget এর master list — নতুন widget যোগ করতে শুধু এখানে একটা entry বাড়ালেই হবে
    public static readonly List<WidgetDefinitionDto> All = new()
    {
        new() { WidgetKey = "stat_today_sales",    Title = "Today's Sales",       Icon = "bi-cash-stack",       Category = "Stats",  AllowResize = false },
        new() { WidgetKey = "stat_total_orders",   Title = "Total Orders",        Icon = "bi-receipt",          Category = "Stats",  AllowResize = false },
        new() { WidgetKey = "stat_low_stock",      Title = "Low Stock Items",     Icon = "bi-exclamation-triangle", Category = "Stats", AllowResize = false },
        new() { WidgetKey = "stat_outstanding",    Title = "Outstanding Due",     Icon = "bi-wallet2",          Category = "Stats",  AllowResize = false },
        new() { WidgetKey = "chart_sales_trend",   Title = "Sales Trend Chart",   Icon = "bi-bar-chart",        Category = "Charts", AllowResize = true  },
        new() { WidgetKey = "recent_activity",     Title = "Recent Activity",     Icon = "bi-clock-history",    Category = "Lists",  AllowResize = true  },
        new() { WidgetKey = "quick_actions",       Title = "Quick Actions",       Icon = "bi-lightning",        Category = "Lists",  AllowResize = true  },
        new() { WidgetKey = "top_products",        Title = "Top Selling Products",Icon = "bi-trophy",           Category = "Lists",  AllowResize = true  },
        new() { WidgetKey = "my_commission",       Title = "My Commission",       Icon = "bi-person-badge",     Category = "Stats",  AllowResize = false },
        new() { WidgetKey = "low_stock_table",     Title = "Low Stock Detail",    Icon = "bi-box-seam",         Category = "Lists",  AllowResize = true  },
    };

    // Default layout — প্রথমবার login করলে এটা দেখাবে
    public static List<WidgetConfigDto> GetDefault() => new()
    {
        new() { WidgetKey = "stat_today_sales",  Order = 0, IsVisible = true,  Size = "Small" },
        new() { WidgetKey = "stat_total_orders", Order = 1, IsVisible = true,  Size = "Small" },
        new() { WidgetKey = "stat_low_stock",    Order = 2, IsVisible = true,  Size = "Small" },
        new() { WidgetKey = "stat_outstanding",  Order = 3, IsVisible = true,  Size = "Small" },
        new() { WidgetKey = "chart_sales_trend", Order = 4, IsVisible = true,  Size = "Large" },
        new() { WidgetKey = "recent_activity",   Order = 5, IsVisible = true,  Size = "Medium" },
        new() { WidgetKey = "quick_actions",     Order = 6, IsVisible = true,  Size = "Full" },
        new() { WidgetKey = "top_products",      Order = 7, IsVisible = false, Size = "Medium" },
        new() { WidgetKey = "my_commission",     Order = 8, IsVisible = false, Size = "Small" },
        new() { WidgetKey = "low_stock_table",   Order = 9, IsVisible = false, Size = "Large" },
    };
}