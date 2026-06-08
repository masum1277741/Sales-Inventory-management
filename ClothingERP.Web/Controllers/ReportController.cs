namespace ClothingERP.Web.Controllers;

public class ReportController : BaseController
{
    private readonly IReportService _reportSvc;
    private readonly ISalesService _salesSvc;
    private readonly IPurchaseService _purchaseSvc;
    private readonly IStockService _stockSvc;
    private readonly ICustomerService _customerSvc;
    private readonly ISupplierService _supplierSvc;
    private readonly IAccountService _accountSvc;

    public ReportController(IReportService reportSvc, ISalesService salesSvc,
        IPurchaseService purchaseSvc, IStockService stockSvc,
        ICustomerService customerSvc, ISupplierService supplierSvc,
        IAccountService accountSvc)
    {
        _reportSvc = reportSvc; _salesSvc = salesSvc;
        _purchaseSvc = purchaseSvc; _stockSvc = stockSvc;
        _customerSvc = customerSvc; _supplierSvc = supplierSvc;
        _accountSvc = accountSvc;
    }

    // ── Hub ───────────────────────────────────────────────────────────────
    public IActionResult Index()
    {
        ViewData["Title"] = "Reports";
        return View();
    }

    // ── Sales Report ──────────────────────────────────────────────────────
    public async Task<IActionResult> SalesReport(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Sales Report";
        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;

        var invoices = (await _salesSvc.GetAllAsync())
            .Where(i => i.Status != "Cancelled" && !i.IsHold &&
                        i.InvoiceDate.Date >= from && i.InvoiceDate.Date <= to)
            .OrderByDescending(i => i.InvoiceDate).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.TotalRevenue = invoices.Sum(i => i.TotalAmount);
        ViewBag.TotalPaid = invoices.Sum(i => i.PaidAmount);
        ViewBag.TotalDue = invoices.Sum(i => i.DueAmount);
        ViewBag.InvoiceCount = invoices.Count;
        ViewBag.AvgSale = invoices.Any() ? invoices.Average(i => i.TotalAmount) : 0;
        ViewBag.TodaySales = invoices.Where(i => i.InvoiceDate.Date == DateTime.Today).Sum(i => i.TotalAmount);

        // Daily chart data (last 30 days)
        var dailyData = Enumerable.Range(0, (to - from).Days + 1)
            .Select(d => from.AddDays(d))
            .Select(date => new {
                date = date.ToString("dd MMM"),
                total = invoices.Where(i => i.InvoiceDate.Date == date).Sum(i => i.TotalAmount)
            }).ToList();

        ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(dailyData.Select(d => d.date));
        ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(dailyData.Select(d => d.total));

        return View(invoices);
    }

    // ── Purchase Report ───────────────────────────────────────────────────
    public async Task<IActionResult> PurchaseReport(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Purchase Report";
        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;

        var orders = (await _purchaseSvc.GetAllOrdersAsync())
            .Where(o => o.Status != "Cancelled" &&
                        o.OrderDate.Date >= from && o.OrderDate.Date <= to)
            .OrderByDescending(o => o.OrderDate).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.TotalAmount = orders.Sum(o => o.TotalAmount);
        ViewBag.TotalPaid = orders.Sum(o => o.PaidAmount);
        ViewBag.TotalDue = orders.Sum(o => o.DueAmount);
        ViewBag.OrderCount = orders.Count;
        ViewBag.ReceivedCount = orders.Count(o => o.Status is "FullyReceived" or "PartiallyReceived");

        return View(orders);
    }

    // ── Stock Report ──────────────────────────────────────────────────────
    public async Task<IActionResult> StockReport()
    {
        ViewData["Title"] = "Stock Report";
        var stock = (await _stockSvc.GetAllAsync()).ToList();

        ViewBag.TotalItems = stock.Count;
        ViewBag.InStock = stock.Count(s => s.Status == "In Stock");
        ViewBag.LowStock = stock.Count(s => s.Status == "Low Stock");
        ViewBag.OutOfStock = stock.Count(s => s.Status == "Out of Stock");
        ViewBag.TotalValue = stock.Sum(s => s.StockValue);

        // Category breakdown
        var categoryBreakdown = stock
            .GroupBy(s => s.CategoryName)
            .Select(g => new { Category = g.Key, Count = g.Count(), Value = g.Sum(x => x.StockValue) })
            .OrderByDescending(x => x.Value).ToList();

        ViewBag.CategoryBreakdown = categoryBreakdown;
        ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(categoryBreakdown.Select(c => c.Category));
        ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(categoryBreakdown.Select(c => c.Value));

        return View(stock.OrderBy(s => s.Status == "Out of Stock" ? 0 : s.Status == "Low Stock" ? 1 : 2)
                         .ThenBy(s => s.Quantity).ToList());
    }

    // ── Customer Due Report ───────────────────────────────────────────────
    public async Task<IActionResult> CustomerDue()
    {
        ViewData["Title"] = "Customer Due Report";
        var customers = (await _customerSvc.GetAllAsync())
            .Where(c => c.CurrentBalance > 0)
            .OrderByDescending(c => c.CurrentBalance).ToList();

        ViewBag.TotalDue = customers.Sum(c => c.CurrentBalance);
        ViewBag.TotalCount = customers.Count;
        ViewBag.Above5k = customers.Count(c => c.CurrentBalance > 5000);
        ViewBag.Above1k = customers.Count(c => c.CurrentBalance is > 1000 and <= 5000);
        ViewBag.Below1k = customers.Count(c => c.CurrentBalance <= 1000);

        return View(customers);
    }

    // ── Supplier Due Report ───────────────────────────────────────────────
    public async Task<IActionResult> SupplierDue()
    {
        ViewData["Title"] = "Supplier Due Report";
        var suppliers = (await _supplierSvc.GetAllAsync())
            .Where(s => s.CurrentBalance > 0)
            .OrderByDescending(s => s.CurrentBalance).ToList();

        ViewBag.TotalDue = suppliers.Sum(s => s.CurrentBalance);
        ViewBag.TotalCount = suppliers.Count;

        return View(suppliers);
    }

    // ── Profit & Loss ─────────────────────────────────────────────────────
    public async Task<IActionResult> ProfitLoss(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Profit & Loss";
        var from = fromDate ?? new DateTime(DateTime.Today.Year, 1, 1);
        var to = toDate ?? DateTime.Today;

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");

        // ── Revenue ───────────────────────────────────────────────────────
        var invoices = (await _salesSvc.GetAllAsync())
            .Where(i => i.Status != "Cancelled" && !i.IsHold &&
                        i.InvoiceDate.Date >= from && i.InvoiceDate.Date <= to).ToList();

        var grossRevenue = invoices.Sum(i => i.TotalAmount);

        // Sales returns deduction
        var salesReturns = await _reportSvc.GetSalesReturnsTotalAsync(from, to);

        // ── COGS ──────────────────────────────────────────────────────────
        var cogs = await _reportSvc.GetCOGSAsync(from, to);

        // ── Operating Expenses ─────────────────────────────────────────────
        var allTxns = (await _accountSvc.GetAllAsync()).ToList();
        var expenses = allTxns
            .Where(t => t.TransactionType == "Expense" &&
                        t.TransactionDate.Date >= from && t.TransactionDate.Date <= to)
            .ToList();
        var otherIncome = allTxns
            .Where(t => t.TransactionType == "Income" &&
                        t.TransactionDate.Date >= from && t.TransactionDate.Date <= to)
            .ToList();

        // ── Summary calculations ───────────────────────────────────────────
        var netRevenue = grossRevenue - salesReturns;
        var grossProfit = netRevenue - cogs;
        var totalExpenses = expenses.Sum(e => e.Amount);
        var totalOtherIncome = otherIncome.Sum(e => e.Amount);
        var netProfit = grossProfit - totalExpenses + totalOtherIncome;

        // Expense groups
        var expenseGroups = expenses
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
            .OrderByDescending(g => g.Total).ToList();

        ViewBag.GrossRevenue = grossRevenue;
        ViewBag.SalesReturns = salesReturns;
        ViewBag.NetRevenue = netRevenue;
        ViewBag.COGS = cogs;
        ViewBag.GrossProfit = grossProfit;
        ViewBag.GrossProfitMargin = netRevenue > 0 ? (grossProfit / netRevenue) * 100 : 0;
        ViewBag.ExpenseGroups = expenseGroups;
        ViewBag.TotalExpenses = totalExpenses;
        ViewBag.OtherIncome = otherIncome;
        ViewBag.TotalOtherIncome = totalOtherIncome;
        ViewBag.NetProfit = netProfit;
        ViewBag.NetProfitMargin = netRevenue > 0 ? (netProfit / netRevenue) * 100 : 0;
        ViewBag.InvoiceCount = invoices.Count;

        return View();
    }
}