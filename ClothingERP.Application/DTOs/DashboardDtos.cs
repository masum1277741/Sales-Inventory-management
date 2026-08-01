namespace ClothingERP.Application.DTOs;

public class DashboardDto
{
    public decimal TodaySales { get; set; }
    public decimal TodayProfit { get; set; }
    public decimal TodayCollection { get; set; }
    public int TodayInvoiceCount { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal TotalCustomerDue { get; set; }
    public decimal TotalSupplierDue { get; set; }
    public decimal TotalStockValue { get; set; }
    public decimal TotalRetailStockValue { get; set; }
    public decimal TotalCostStockValue { get; set; }
    public int TotalActiveCustomers { get; set; }
    public List<MonthlySalesChartDto> MonthlySalesChart { get; set; } = new();
    public List<TopProductDto> TopSellingProducts { get; set; } = new();
    public List<SalesInvoiceListDto> RecentInvoices { get; set; } = new();
    public List<StockListDto> LowStockAlerts { get; set; } = new();
}

public class MonthlySalesChartDto
{
    public string Month { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
    public decimal ProfitAmount { get; set; }
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}