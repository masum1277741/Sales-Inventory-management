namespace ClothingERP.Application.DTOs;

public class SalesReportItemDto
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StockReportItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal StockValue { get; set; }
    public string StockStatus { get; set; } = string.Empty;
}

public class PurchaseReportItemDto
{
    public int PurchaseOrderId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DueAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ProfitLossDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetSales { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitPercent => NetSales > 0 ? Math.Round(GrossProfit / NetSales * 100, 2) : 0;
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
    public decimal NetProfitPercent => NetSales > 0 ? Math.Round(NetProfit / NetSales * 100, 2) : 0;
    public List<ExpenseSummaryDto> ExpenseBreakdown { get; set; } = new();
}

public class ExpenseSummaryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SalesReturnReportItemDto
{
    public string ReturnNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RefundAmount { get; set; }
}